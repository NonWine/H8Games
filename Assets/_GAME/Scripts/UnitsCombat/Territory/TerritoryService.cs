using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TerritoryService : IInitializable, ITickable, IDisposable
{
    private class TrackedUnit
    {
        public EnemyCombatAgentController Controller;
        public Vector3                    TargetPosition;
        public Vector3                    SmoothedPosition;
        public Vector3                    SmoothVelocity;
        public bool                       WasSeenThisScan;
        public bool                       IsDying;
    }

    private readonly TerritoryView   view;
    private readonly TerritoryConfig config;
    private readonly LevelManager    levelManager;

    private readonly Dictionary<EnemyCombatAgentController, TrackedUnit> trackedUnits = new();
    private readonly List<Vector3>                                        smoothedPositions = new();
    private readonly List<EnemyCombatAgentController>                     removalBuffer = new();

    private float scanTimer;

    [Inject]
    public TerritoryService(TerritoryView view, TerritoryConfig config, LevelManager levelManager)
    {
        this.view         = view;
        this.config       = config;
        this.levelManager = levelManager;
    }

    public void Initialize()
    {
        scanTimer = config.UpdateInterval;
    }

    public void Tick()
    {
        float dt = Time.deltaTime;

        scanTimer += dt;
        bool scanChanged = false;
        if (scanTimer >= config.UpdateInterval)
        {
            scanTimer   = 0f;
            scanChanged = ScanUnits();
        }

        bool positionsChanged = UpdateSmoothedPositions(dt);
        if (positionsChanged || scanChanged)
            Refresh();
    }

    public void Dispose()
    {
        view.Clear();
        trackedUnits.Clear();
    }

    private bool ScanUnits()
    {
        int countBefore = trackedUnits.Count;

        foreach (TrackedUnit unit in trackedUnits.Values)
            unit.WasSeenThisScan = false;

        LevelRuntime currentLevel = levelManager.CurrentLevel;
        if (currentLevel == null)
            return false;

        IReadOnlyList<EnemyGroupViewController> groups = currentLevel.Groups;
        for (int g = 0; g < groups.Count; g++)
        {
            EnemyGroupViewController group = groups[g];
            if (group == null || group.State == EnemyGroupState.Cleared)
                continue;

            IReadOnlyList<EnemyCombatAgentController> enemies = group.Enemies;
            for (int e = 0; e < enemies.Count; e++)
            {
                EnemyCombatAgentController enemy = enemies[e];
                if (enemy == null || !enemy.IsAlive)
                    continue;

                Vector3 worldPos = enemy.Position;
                if (trackedUnits.TryGetValue(enemy, out TrackedUnit unit))
                {
                    if (Vector3.Distance(unit.TargetPosition, worldPos) > config.SnapDistance)
                        unit.SmoothVelocity = Vector3.zero;
                    unit.TargetPosition  = worldPos;
                    unit.WasSeenThisScan = true;
                }
                else
                {
                    trackedUnits[enemy] = new TrackedUnit
                    {
                        Controller       = enemy,
                        TargetPosition   = worldPos,
                        SmoothedPosition = worldPos,
                        WasSeenThisScan  = true,
                    };
                }
            }
        }

        foreach (TrackedUnit unit in trackedUnits.Values)
        {
            if (!unit.WasSeenThisScan && !unit.IsDying)
                unit.IsDying = true;
        }

        return trackedUnits.Count != countBefore;
    }

    private bool UpdateSmoothedPositions(float dt)
    {
        if (trackedUnits.Count == 0)
            return false;

        bool    hasChanges    = false;
        Vector3 aliveCentroid = ComputeAliveCentroid();

        removalBuffer.Clear();

        foreach (KeyValuePair<EnemyCombatAgentController, TrackedUnit> kvp in trackedUnits)
        {
            TrackedUnit unit = kvp.Value;

            if (unit.IsDying)
                unit.TargetPosition = aliveCentroid;

            float dist = Vector3.Distance(unit.SmoothedPosition, unit.TargetPosition);
            if (dist <= config.SnapDistance)
            {
                if (unit.IsDying)
                    removalBuffer.Add(kvp.Key);
                continue;
            }

            float   smoothTime = unit.IsDying ? config.ShrinkDuration : config.ExpandDuration;
            Vector3 vel        = unit.SmoothVelocity;
            unit.SmoothedPosition = Vector3.SmoothDamp(
                unit.SmoothedPosition, unit.TargetPosition, ref vel, smoothTime, float.MaxValue, dt);
            unit.SmoothVelocity = vel;

            hasChanges = true;
        }

        for (int i = 0; i < removalBuffer.Count; i++)
            trackedUnits.Remove(removalBuffer[i]);

        if (removalBuffer.Count > 0)
            hasChanges = true;

        return hasChanges;
    }

    private Vector3 ComputeAliveCentroid()
    {
        Vector3 sum   = Vector3.zero;
        int     count = 0;

        foreach (TrackedUnit unit in trackedUnits.Values)
        {
            if (!unit.IsDying) { sum += unit.TargetPosition; count++; }
        }

        if (count > 0) return sum / count;

        foreach (TrackedUnit unit in trackedUnits.Values)
        {
            sum += unit.SmoothedPosition; count++;
        }

        return count > 0 ? sum / count : Vector3.zero;
    }

    private void Refresh()
    {
        smoothedPositions.Clear();
        foreach (TrackedUnit unit in trackedUnits.Values)
            smoothedPositions.Add(unit.SmoothedPosition);

        view.Refresh(smoothedPositions, config);
    }
}
