using System.Collections.Generic;
using UnityEngine;

public sealed class CampaignService
{
    private const string CurrentLevelIndexPrefsKey = "campaign.current_level_index";

    private readonly List<LevelRuntime> levels;

    public CampaignService(IEnumerable<LevelRuntime> levels, int currentLevelIndex)
    {
        this.levels = levels != null ? new List<LevelRuntime>(levels) : new List<LevelRuntime>();

        int savedLevelIndex = PlayerPrefs.GetInt(CurrentLevelIndexPrefsKey, currentLevelIndex);
        CurrentLevelIndex = this.levels.Count == 0
            ? -1
            : Mathf.Clamp(savedLevelIndex, 0, this.levels.Count - 1);

        ApplyLevelActivation();
    }

    public IReadOnlyList<LevelRuntime> Levels => levels;
    public int CurrentLevelIndex { get; private set; }
    public LevelRuntime CurrentLevel =>
        CurrentLevelIndex >= 0 && CurrentLevelIndex < levels.Count ? levels[CurrentLevelIndex] : null;

    public bool TrySetCurrentLevelIndex(int index)
    {
        if (index < 0 || index >= levels.Count)
            return false;

        CurrentLevelIndex = index;
        PlayerPrefs.SetInt(CurrentLevelIndexPrefsKey, CurrentLevelIndex);
        PlayerPrefs.Save();
        ApplyLevelActivation();
        return true;
    }

    public bool TryAdvanceToNextLevelOrRestart()
    {
        if (levels.Count == 0)
            return false;

        int nextLevelIndex = CurrentLevelIndex + 1;
        if (nextLevelIndex < levels.Count)
            return TrySetCurrentLevelIndex(nextLevelIndex);

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        CurrentLevelIndex = 0;
        ApplyLevelActivation();
        return true;
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

            if (shouldBeActive)
                level.ResetRuntimeState();
        }
    }
}
