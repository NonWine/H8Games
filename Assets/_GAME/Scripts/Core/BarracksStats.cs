using System;
using UnityEngine;

[Serializable]
public class BarracksStats
{
    [Min(0.15f)] public float SpawnInterval = 2.5f;
    [SerializeField] private BarrackLevelData[]  levels;
    [SerializeField] private int startLevel = 0;
    private int currentLevel;
    
    public int CurrentLevel => currentLevel;
    
    public UnitCombatDefinition Unit => levels[startLevel].UnitCombatDefinition;
    
    public BarracksStats()
    {
        currentLevel = startLevel;
    }

    public BarracksStats(BarracksStats source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        SpawnInterval = source.SpawnInterval;
    }

    public void Update() => currentLevel++;

}
[System.Serializable]
public struct BarrackLevelData
{
    public UnitCombatDefinition UnitCombatDefinition;
}