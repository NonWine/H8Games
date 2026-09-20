using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class LevelManager : IInitializable, IDisposable
{
    private const string CurrentLevelIndexPrefsKey = "campaign.current_level_index";

    private readonly SignalBus signalBus;
    private readonly List<LevelRuntime> levels;

    public LevelManager(SignalBus signalBus, IEnumerable<LevelRuntime> levels, int currentLevelIndex)
    {
        this.signalBus = signalBus;
        this.levels = levels != null ? new List<LevelRuntime>(levels) : new List<LevelRuntime>();

        int savedLevelIndex = PlayerPrefs.GetInt(CurrentLevelIndexPrefsKey, currentLevelIndex);
        CurrentLevelIndex = this.levels.Count == 0
            ? -1
            : Mathf.Clamp(savedLevelIndex, 0, this.levels.Count - 1);

    }

    public IReadOnlyList<LevelRuntime> Levels => levels;
    public int CurrentLevelIndex { get; private set; }
    public LevelRuntime CurrentLevel => CurrentLevelIndex >= 0 && CurrentLevelIndex < levels.Count ? levels[CurrentLevelIndex] : null;

    public void TrySetCurrentLevelIndex(int index)
    {
        if (index < 0 || index >= levels.Count)
            return;

        CurrentLevelIndex = index;
        PlayerPrefs.SetInt(CurrentLevelIndexPrefsKey, CurrentLevelIndex);
        PlayerPrefs.Save();
        ApplyLevelActivation();
    }

    public void Initialize()
    {
        signalBus.Subscribe<LoadNextLevelSignal>(TryAdvanceToNextLevelOrRestart);
        ApplyLevelActivation();
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<LoadNextLevelSignal>(TryAdvanceToNextLevelOrRestart);
    }

    public void TryAdvanceToNextLevelOrRestart()
    {
        if (levels.Count == 0)
            return;

        int nextLevelIndex = CurrentLevelIndex + 1;
        if (nextLevelIndex < levels.Count)
        {
            TrySetCurrentLevelIndex(nextLevelIndex);
            return;
        }

        RestartCampaign();
    }

    // Looping back to level 0 used to reload the whole scene - the only scene
    // reload in the project, and a visible hitch for something that only needs
    // to re-arm every level's own enemy groups, same as a defeat already does
    // for one level via ResetRuntimeState().
    //
    // Each level is briefly activated for its own reset rather than reset while
    // inactive: EnemyCombatAgentController.Spawn() warps a dead enemy's
    // NavMeshAgent back to its spawn pose, and Warp() is a no-op on an agent
    // that isn't currently enabled in the hierarchy.
    private void RestartCampaign()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        for (int i = 0; i < levels.Count; i++)
        {
            LevelRuntime level = levels[i];
            if (level == null)
                continue;

            level.gameObject.SetActive(true);
            level.ResetRuntimeState();
        }

        CurrentLevelIndex = 0;
        ApplyLevelActivation();
        signalBus.Fire<LevelRestartedSignal>();
    }

    private void ApplyLevelActivation()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            LevelRuntime level = levels[i];
            if (level == null)
                continue;

            bool shouldBeActive = i == CurrentLevelIndex;
            level.gameObject.SetActive(shouldBeActive);
        }
    }
}
