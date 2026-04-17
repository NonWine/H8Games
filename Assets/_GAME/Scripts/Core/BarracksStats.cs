using System;
using UnityEngine;

[Serializable]
public class BarracksStats
{
    [Min(0.15f)] public float SpawnInterval = 2.5f;
    [SerializeField] private BarrackLevelData[]  levels;
    [SerializeField] private int startLevel = 0;
    private int currentLevel;
    
    public BarrackLevelData BarrackLevelData => levels[currentLevel];
    
    public int MaxLevel => levels == null || levels.Length == 0 ? 0 : levels.Length - 1;
    public bool IsMaxLevel => currentLevel >= MaxLevel;
    public UnitCombatDefinition Unit => GetLevelData(currentLevel).UnitCombatDefinition;
    
    public BarracksStats()
    {
        ResetRuntimeState();
    }

    public BarracksStats(BarracksStats source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        SpawnInterval = source.SpawnInterval;
    }

    public void ResetRuntimeState()
    {
        currentLevel = Mathf.Clamp(startLevel, 0, MaxLevel);
    }

    public void Update()
    {
        if (IsMaxLevel)
            return;

        currentLevel = Mathf.Min(currentLevel + 1, MaxLevel);
    }

    private BarrackLevelData GetLevelData(int level)
    {
        if (levels == null || levels.Length == 0)
            return new BarrackLevelData();

        int clampedLevel = Mathf.Clamp(level, 0, levels.Length - 1);
        return levels[clampedLevel];
    }

}
[System.Serializable]
public struct BarrackLevelData
{
    public UnitCombatDefinition UnitCombatDefinition;
    public GameObject UnitModel;
}
