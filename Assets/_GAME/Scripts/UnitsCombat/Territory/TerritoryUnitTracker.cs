using System.Collections.Generic;
using UnityEngine;

public class TerritoryUnitTracker
{
    private sealed class TrackedUnit
    {
        public EnemyCombatAgentController Source;
        public Vector3 Target;
        public Vector3 Current;
        public Vector3 Velocity;
    }

    private readonly TerritoryConfig config;
    private readonly LevelManager levelManager;
    private readonly List<TrackedUnit> units = new();
    private readonly List<EnemyCombatAgentController> tempAlive = new();
    private readonly List<Vector3> smoothedPositions = new();

    private LevelRuntime activeLevel;

    public IReadOnlyList<Vector3> SmoothedPositions => smoothedPositions;

    public TerritoryUnitTracker(TerritoryConfig config, LevelManager levelManager)
    {
        this.config = config;
        this.levelManager = levelManager;
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

        CollectAliveEnemies(current);

        bool changed = tempAlive.Count != CountActiveUnits();

        RemoveDeadUnits();
        AddNewUnits();

        return changed;
    }

    public bool UpdatePositions(float dt)
    {
        bool changed = false;
        smoothedPositions.Clear();

        for (int i = 0; i < units.Count; i++)
        {
            TrackedUnit unit = units[i];

            if (unit.Source != null && unit.Source.IsAlive)
                unit.Target = unit.Source.Position;

            Vector3 newPos = Vector3.SmoothDamp(
                unit.Current, unit.Target, ref unit.Velocity,
                config.ExpandDuration, float.MaxValue, dt);

            if ((newPos - unit.Current).sqrMagnitude > config.SnapDistance * config.SnapDistance)
                changed = true;

            unit.Current = newPos;
            smoothedPositions.Add(newPos);
        }

        return changed;
    }

    public void Reset()
    {
        units.Clear();
        smoothedPositions.Clear();
    }

    private void CollectAliveEnemies(LevelRuntime level)
    {
        tempAlive.Clear();

        for (int g = 0; g < level.Groups.Count; g++)
        {
            EnemyGroupViewController group = level.Groups[g];
            if (group == null)
                continue;

            IReadOnlyList<EnemyCombatAgentController> enemies = group.Enemies;
            for (int e = 0; e < enemies.Count; e++)
            {
                EnemyCombatAgentController enemy = enemies[e];
                if (enemy != null && enemy.IsAlive)
                    tempAlive.Add(enemy);
            }
        }
    }

    private void RemoveDeadUnits()
    {
        for (int i = units.Count - 1; i >= 0; i--)
        {
            bool stillAlive = false;
            for (int j = 0; j < tempAlive.Count; j++)
            {
                if (ReferenceEquals(units[i].Source, tempAlive[j]))
                {
                    stillAlive = true;
                    break;
                }
            }

            if (!stillAlive)
                units.RemoveAt(i);
        }
    }

    private void AddNewUnits()
    {
        for (int i = 0; i < tempAlive.Count; i++)
        {
            bool alreadyTracked = false;
            for (int j = 0; j < units.Count; j++)
            {
                if (ReferenceEquals(units[j].Source, tempAlive[i]))
                {
                    alreadyTracked = true;
                    break;
                }
            }

            if (!alreadyTracked)
            {
                Vector3 spawnPos = tempAlive[i].Position;
                units.Add(new TrackedUnit
                {
                    Source = tempAlive[i],
                    Target = spawnPos,
                    Current = spawnPos,
                    Velocity = Vector3.zero
                });
            }
        }
    }

    private int CountActiveUnits()
    {
        return units.Count;
    }
}
