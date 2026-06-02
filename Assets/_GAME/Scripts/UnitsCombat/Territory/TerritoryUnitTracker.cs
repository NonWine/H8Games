using System.Collections.Generic;
using UnityEngine;

public class TerritoryUnitTracker
{
    private class TrackedUnit
    {
        public Vector3 TargetPosition;
        public Vector3 SmoothedPosition;
        public bool    WasSeenThisScan;
    }

    private readonly TerritoryConfig  config;
    private readonly LevelManager     levelManager;
    private          Transform        flagAnchor;

    private readonly Dictionary<EnemyCombatAgentController, TrackedUnit> trackedUnits      = new();
    private readonly List<EnemyCombatAgentController>                    removalBuffer     = new();
    private readonly List<Vector3>                                       smoothedPositions = new();

    private LevelRuntime activeLevel;

    public IReadOnlyList<Vector3> SmoothedPositions => smoothedPositions;

    public TerritoryUnitTracker(TerritoryConfig config, LevelManager levelManager, Transform flagAnchor = null)
    {
        this.config       = config;
        this.levelManager = levelManager;
        this.flagAnchor   = flagAnchor;
    }

    public void SetFlagAnchor(Transform anchor)
    {
        flagAnchor = anchor;
    }

    public bool Scan(out bool levelChanged)
    {
        LevelRuntime current = levelManager.CurrentLevel;
        levelChanged = !ReferenceEquals(current, activeLevel);

        if (levelChanged)
        {
            activeLevel = current;
            Reset();
            return true;
        }

        if (current == null)
            return false;

        MarkAllAsNotSeen();
        ScanLevel(current);
        RemoveStaleUnits();

        return true;
    }

    public bool UpdatePositions(float dt)
    {
        if (trackedUnits.Count == 0)
        {
            // No live units — keep the flag visible if present.
            smoothedPositions.Clear();
            if (flagAnchor != null)
                smoothedPositions.Add(flagAnchor.position);
            return false;
        }

        bool    hasChanges  = false;
        float   snapDistSqr = config.SnapDistance * config.SnapDistance;
        Vector3 anchor      = ComputeTargetCentroid();

        float expandSpeed = config.ExpandDuration > 0f ? 4f / config.ExpandDuration : 16f;
        float shrinkSpeed = config.ShrinkDuration > 0f ? 4f / config.ShrinkDuration : 10f;

        foreach (KeyValuePair<EnemyCombatAgentController, TrackedUnit> kvp in trackedUnits)
        {
            TrackedUnit unit    = kvp.Value;
            Vector3     current = unit.SmoothedPosition;
            Vector3     target  = unit.TargetPosition;

            if (!IsFinite(target))
                continue;

            if (!IsFinite(current))
            {
                unit.SmoothedPosition = target;
                hasChanges            = true;
                continue;
            }

            Vector3 delta = target - current;

            if (delta.sqrMagnitude <= snapDistSqr)
            {
                if ((current - target).sqrMagnitude > 1e-6f)
                {
                    unit.SmoothedPosition = target;
                    hasChanges            = true;
                }
                continue;
            }

            float   speed = ResolveSmoothSpeed(unit, anchor, expandSpeed, shrinkSpeed);
            float   t     = speed <= 0f ? 1f : 1f - Mathf.Exp(-speed * dt);
            Vector3 next  = Vector3.Lerp(current, target, t);

            if (!IsFinite(next))
            {
                unit.SmoothedPosition = target;
                hasChanges            = true;
                continue;
            }

            if ((next - current).sqrMagnitude <= 1e-6f)
                continue;

            unit.SmoothedPosition = next;
            hasChanges            = true;
        }

        smoothedPositions.Clear();

        if (flagAnchor != null)
            smoothedPositions.Add(flagAnchor.position);

        foreach (TrackedUnit unit in trackedUnits.Values)
            smoothedPositions.Add(unit.SmoothedPosition);

        return hasChanges;
    }

    public void Reset()
    {
        trackedUnits.Clear();
        smoothedPositions.Clear();
    }

    private void MarkAllAsNotSeen()
    {
        foreach (TrackedUnit unit in trackedUnits.Values)
            unit.WasSeenThisScan = false;
    }

    private void ScanLevel(LevelRuntime level)
    {
        Vector3 spawnOrigin = ComputeSpawnOrigin(level);

        for (int g = 0; g < level.Groups.Count; g++)
        {
            EnemyGroupViewController group = level.Groups[g];
            if (group == null)
                continue;

            IReadOnlyList<EnemyCombatAgentController> enemies = group.Enemies;
            for (int e = 0; e < enemies.Count; e++)
            {
                EnemyCombatAgentController enemy = enemies[e];

                if (enemy == null || !enemy.IsAlive)
                    continue;

                Vector3 pos = enemy.Position;
                if (!IsFinite(pos))
                    continue;

                if (trackedUnits.TryGetValue(enemy, out TrackedUnit existing))
                {
                    existing.TargetPosition  = pos;
                    existing.WasSeenThisScan = true;
                }
                else
                {
                    trackedUnits[enemy] = new TrackedUnit
                    {
                        TargetPosition   = pos,
                        SmoothedPosition = spawnOrigin,
                        WasSeenThisScan  = true
                    };
                }
            }
        }
    }

    private void RemoveStaleUnits()
    {
        removalBuffer.Clear();

        foreach (KeyValuePair<EnemyCombatAgentController, TrackedUnit> kvp in trackedUnits)
        {
            if (!kvp.Value.WasSeenThisScan)
                removalBuffer.Add(kvp.Key);
        }

        for (int i = 0; i < removalBuffer.Count; i++)
            trackedUnits.Remove(removalBuffer[i]);
    }

    private static float ResolveSmoothSpeed(TrackedUnit unit, Vector3 anchor, float expandSpeed, float shrinkSpeed)
    {
        Vector3 currentDelta = unit.SmoothedPosition - anchor;
        Vector3 targetDelta  = unit.TargetPosition   - anchor;
        currentDelta.y = 0f;
        targetDelta.y  = 0f;

        return targetDelta.sqrMagnitude >= currentDelta.sqrMagnitude ? expandSpeed : shrinkSpeed;
    }

    private Vector3 ComputeTargetCentroid()
    {
        Vector3 sum = Vector3.zero;
        foreach (TrackedUnit unit in trackedUnits.Values)
            sum += unit.TargetPosition;
        return trackedUnits.Count > 0 ? sum / trackedUnits.Count : Vector3.zero;
    }

    private Vector3 ComputeSpawnOrigin(LevelRuntime level)
    {
        if (trackedUnits.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (TrackedUnit unit in trackedUnits.Values)
                sum += unit.SmoothedPosition;
            return sum / trackedUnits.Count;
        }

        Vector3 newSum   = Vector3.zero;
        int     newCount = 0;

        for (int g = 0; g < level.Groups.Count; g++)
        {
            EnemyGroupViewController group = level.Groups[g];
            if (group == null)
                continue;

            IReadOnlyList<EnemyCombatAgentController> enemies = group.Enemies;
            for (int e = 0; e < enemies.Count; e++)
            {
                EnemyCombatAgentController enemy = enemies[e];
                if (enemy == null || !enemy.IsAlive)
                    continue;

                Vector3 pos = enemy.Position;
                if (!IsFinite(pos))
                    continue;

                newSum += pos;
                newCount++;
            }
        }

        return newCount > 0 ? newSum / newCount : Vector3.zero;
    }

    private static bool IsFinite(Vector3 v)
    {
        return !float.IsNaN(v.x)      && !float.IsNaN(v.y)      && !float.IsNaN(v.z)
            && !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z);
    }
}
