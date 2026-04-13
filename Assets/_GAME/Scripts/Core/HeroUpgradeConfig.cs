using System;
using UnityEngine;

[Serializable]
public class HeroUpgradeConfig
{
    [SerializeField] private UpgradeDefinition damage = new();
    [SerializeField] private UpgradeDefinition maxHealth = new();
    [SerializeField] private UpgradeDefinition attackRate = new();

    public UpgradeDefinition Damage => damage;
    public UpgradeDefinition MaxHealth => maxHealth;
    public UpgradeDefinition AttackRate => attackRate;

    public HeroUpgradeConfig()
    {
    }

    public HeroUpgradeConfig(HeroUpgradeConfig source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        damage = source.damage != null ? new UpgradeDefinition(source.damage) : new UpgradeDefinition();
        maxHealth = source.maxHealth != null ? new UpgradeDefinition(source.maxHealth) : new UpgradeDefinition();
        attackRate = source.attackRate != null ? new UpgradeDefinition(source.attackRate) : new UpgradeDefinition();
    }

    public UpgradeDefinition GetDefinition(UpgradeKind kind)
    {
        return kind switch
        {
            UpgradeKind.HeroDamage => damage,
            UpgradeKind.HeroMaxHealth => maxHealth,
            UpgradeKind.HeroAttackRate => attackRate,
            _ => null
        };
    }
}
