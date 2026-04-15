using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class LevelManager : IInitializable, IDisposable
{
    private const string CurrentLevelIndexPrefsKey = "campaign.current_level_index";
    private readonly SignalBus signalBus;

    private readonly List<LevelRuntime> levels;
    private SquadRoot squadRoot;

    public LevelManager(SignalBus signalBus, IEnumerable<LevelRuntime> levels, int currentLevelIndex)
    {
        this.signalBus = signalBus;
        this.levels = levels != null ? new List<LevelRuntime>(levels) : new List<LevelRuntime>();

        int savedLevelIndex = PlayerPrefs.GetInt(CurrentLevelIndexPrefsKey, currentLevelIndex);
        CurrentLevelIndex = this.levels.Count == 0
            ? -1
            : Mathf.Clamp(savedLevelIndex, 0, this.levels.Count - 1);

        ApplyLevelActivation();
    }

    public IReadOnlyList<LevelRuntime> Levels => levels;
    public int CurrentLevelIndex { get; private set; }
    public LevelRuntime CurrentLevel => CurrentLevelIndex >= 0 && CurrentLevelIndex < levels.Count ? levels[CurrentLevelIndex] : null;

    public void TrySetCurrentLevelIndex(int index)
    {
        CurrentLevelIndex = index;
        PlayerPrefs.SetInt(CurrentLevelIndexPrefsKey, CurrentLevelIndex);
        PlayerPrefs.Save();
        ApplyLevelActivation();
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<LoadNextLevelSignal>(TryAdvanceToNextLevelOrRestart);
    }

    public void Initialize()
    {
        signalBus.Subscribe<LoadNextLevelSignal>(TryAdvanceToNextLevelOrRestart);
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

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        CurrentLevelIndex = 0;
        ApplyLevelActivation();
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
