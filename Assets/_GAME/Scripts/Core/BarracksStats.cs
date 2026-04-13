using System;
using UnityEngine;

[Serializable]
public class BarracksStats
{
    [SerializeField] private UnitStats spawnUnit = new();
    [Min(1f)] public float MaxHealth = 250f;
    [Min(0.15f)] public float SpawnInterval = 2.5f;
    [Min(1)] public int MaxAlive = 5;
    public float UnitHealthMultiplier { get; private set; } = 1f;
    public float UnitDamageMultiplier { get; private set; } = 1f;

    public UnitStats SpawnUnit => spawnUnit;

    public BarracksStats()
    {
    }

    public BarracksStats(BarracksStats source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        spawnUnit = source.spawnUnit != null ? new UnitStats(source.spawnUnit) : new UnitStats();
        MaxHealth = source.MaxHealth;
        SpawnInterval = source.SpawnInterval;
        MaxAlive = source.MaxAlive;
        UnitHealthMultiplier = source.UnitHealthMultiplier;
        UnitDamageMultiplier = source.UnitDamageMultiplier;
    }

    public void ReduceSpawnInterval(float amount)
    {
        SpawnInterval = Mathf.Max(0.15f, SpawnInterval - amount);
    }

    public void MultiplyUnitHealth(float multiplier)
    {
        UnitHealthMultiplier *= Mathf.Max(1f, multiplier);
    }

    public void MultiplyUnitDamage(float multiplier)
    {
        UnitDamageMultiplier *= Mathf.Max(1f, multiplier);
    }

    public UnitStats BuildSpawnStats(UnitStats source = null)
    {
        UnitStats template = source ?? spawnUnit;
        var runtime = template != null ? new UnitStats(template) : new UnitStats();
        runtime.MultiplyMaxHealth(UnitHealthMultiplier);
        runtime.MultiplyDamage(UnitDamageMultiplier);
        return runtime;
    }
}
