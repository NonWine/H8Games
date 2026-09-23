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

    private readonly Dictionary<EnemyCombatAgentController, TrackedUnit> trackedUnits      = new();
    private readonly List<EnemyCombatAgentController>                    removalBuffer     = new();
    private readonly List<Vector3>                                       smoothedPositions = new();

    private LevelRuntime activeLevel;

    // Centroid of the last non-empty set of tracked enemies. Deliberately kept
    // after the set empties: the frame the last enemy dies this still holds the
    // spot it was standing on, which is what the capture zone wants.
    private Vector3 lastKnownCentroid;
    private bool    hasKnownCentroid;

    // While active the zone stops following units and holds a single frozen
    // point, so the boundary smoothing shrinks whatever is on screen into a
    // circle there instead of collapsing the zone away.
    private Vector3 captureFocusPosition;
    private bool    captureFocusActive;

    public IReadOnlyList<Vector3> SmoothedPositions => smoothedPositions;

    public TerritoryUnitTracker(TerritoryConfig config, LevelManager levelManager)
    {
        this.config       = config;
        this.levelManager = levelManager;
    }

    // Resolved on read rather than pushed on a signal, so no subscriber ordering
    // decides whether the caller sees the death spot or a stale one.
    public bool TryGetCaptureFocus(out Vector3 worldPosition)
    {
        if (captureFocusActive)
        {
            worldPosition = captureFocusPosition;
            return true;
        }

        worldPosition = lastKnownCentroid;
        return hasKnownCentroid;
    }

    public void EnterCaptureFocus()
    {
        if (captureFocusActive || !hasKnownCentroid)
            return;

        captureFocusPosition = lastKnownCentroid;
        captureFocusActive   = true;
    }

    public void ExitCaptureFocus()
    {
        captureFocusActive = false;
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
        if (captureFocusActive)
        {
            smoothedPositions.Clear();
            smoothedPositions.Add(captureFocusPosition);
            return false;
        }

        if (trackedUnits.Count == 0)
        {
            smoothedPositions.Clear();
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
        Vector3 centroidSum = Vector3.zero;

        foreach (TrackedUnit unit in trackedUnits.Values)
        {
            smoothedPositions.Add(unit.SmoothedPosition);
            centroidSum += unit.SmoothedPosition;
        }

        lastKnownCentroid = centroidSum / trackedUnits.Count;
        hasKnownCentroid  = true;

        return hasChanges;
    }

    public void Reset()
    {
        trackedUnits.Clear();
        smoothedPositions.Clear();
        captureFocusActive = false;
        hasKnownCentroid   = false;
        lastKnownCentroid  = Vector3.zero;
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
