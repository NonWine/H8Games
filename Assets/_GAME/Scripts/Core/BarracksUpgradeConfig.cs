using System;
using UnityEngine;

[Serializable]
public class BarracksUpgradeConfig
{
    [SerializeField] private UpgradeDefinition spawnSpeed = new();
    [SerializeField] private UpgradeDefinition unitHealth = new();
    [SerializeField] private UpgradeDefinition unitDamage = new();

    public UpgradeDefinition SpawnSpeed => spawnSpeed;
    public UpgradeDefinition UnitHealth => unitHealth;
    public UpgradeDefinition UnitDamage => unitDamage;

    public BarracksUpgradeConfig()
    {
    }

    public BarracksUpgradeConfig(BarracksUpgradeConfig source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        spawnSpeed = source.spawnSpeed != null ? new UpgradeDefinition(source.spawnSpeed) : new UpgradeDefinition();
        unitHealth = source.unitHealth != null ? new UpgradeDefinition(source.unitHealth) : new UpgradeDefinition();
        unitDamage = source.unitDamage != null ? new UpgradeDefinition(source.unitDamage) : new UpgradeDefinition();
    }

    public UpgradeDefinition GetDefinition(UpgradeKind kind)
    {
        return kind switch
        {
            UpgradeKind.BarracksSpawnSpeed => spawnSpeed,
            UpgradeKind.BarracksUnitHealth => unitHealth,
            UpgradeKind.BarracksUnitDamage => unitDamage,
            _ => null
        };
    }
}
