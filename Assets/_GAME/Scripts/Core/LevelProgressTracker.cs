using System;
using System.Collections.Generic;
using Zenject;

// Reports how many of the current level's enemies are dead, not how many
// encounter groups are Cleared - a group can be half-fought when the squad
// wipes, and the bar should still creep forward for every kill it already
// landed. A defeat never touches enemy state (see SquadCombatStateController),
// so this only ever counts up within a level; LevelRestartedSignal is the one
// path that revives everyone and needs a full recount.
public class LevelProgressTracker : IInitializable, IDisposable
{
    private readonly LevelManager levelManager;
    private readonly SignalBus signalBus;
    private readonly List<EnemyCombatAgentController> trackedEnemies = new();

    private int totalEnemyCount;
    private int defeatedEnemyCount;

    public event Action<float> ProgressChanged;

    public float Progress => totalEnemyCount == 0 ? 0f : (float)defeatedEnemyCount / totalEnemyCount;

    public LevelProgressTracker(LevelManager levelManager, SignalBus signalBus)
    {
        this.levelManager = levelManager;
        this.signalBus = signalBus;
    }

    public void Initialize()
    {
        signalBus.Subscribe<LoadNextLevelSignal>(TrackCurrentLevel);
        signalBus.Subscribe<LevelRestartedSignal>(TrackCurrentLevel);
        TrackCurrentLevel();
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<LoadNextLevelSignal>(TrackCurrentLevel);
        signalBus.Unsubscribe<LevelRestartedSignal>(TrackCurrentLevel);
        UnsubscribeEnemies();
    }

    // Re-reads LevelRuntime.Groups each time rather than caching once, so a
    // level change or a full revive both start from the current authored set.
    private void TrackCurrentLevel()
    {
        UnsubscribeEnemies();

        LevelRuntime level = levelManager.CurrentLevel;
        if (level == null)
        {
            totalEnemyCount = 0;
            RecomputeDefeatedCount();
            return;
        }

        level.RebuildGroups();
        IReadOnlyList<EnemyGroupViewController> groups = level.Groups;

        for (int i = 0; i < groups.Count; i++)
        {
            EnemyGroupViewController group = groups[i];
            if (group == null)
            {
                continue;
            }

            IReadOnlyList<EnemyCombatAgentController> enemies = group.Enemies;

            for (int j = 0; j < enemies.Count; j++)
            {
                EnemyCombatAgentController enemy = enemies[j];
                if (enemy == null)
                {
                    continue;
                }

                enemy.Died += HandleEnemyDied;
                trackedEnemies.Add(enemy);
            }
        }

        totalEnemyCount = trackedEnemies.Count;
        RecomputeDefeatedCount();
    }

    private void HandleEnemyDied()
    {
        RecomputeDefeatedCount();
    }

    // Recomputed from live IsAlive state rather than incremented/decremented,
    // so a revive (which fires no event of its own) can never leave this
    // silently out of sync with what actually happened to the enemies.
    private void RecomputeDefeatedCount()
    {
        int count = 0;

        for (int i = 0; i < trackedEnemies.Count; i++)
        {
            EnemyCombatAgentController enemy = trackedEnemies[i];
            if (enemy != null && !enemy.IsAlive)
            {
                count++;
            }
        }

        defeatedEnemyCount = count;
        ProgressChanged?.Invoke(Progress);
    }

    private void UnsubscribeEnemies()
    {
        for (int i = 0; i < trackedEnemies.Count; i++)
        {
            if (trackedEnemies[i] != null)
            {
                trackedEnemies[i].Died -= HandleEnemyDied;
            }
        }

        trackedEnemies.Clear();
    }
}
