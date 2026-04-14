using System;
using UnityEngine;

[Serializable]
public class UnitStats
{
    [Min(1f)] public float MaxHealth = 30f;
    [Min(0f)] public float MoveSpeed = 3.5f;
    [Min(0.25f)] public float DetectionRadius = 8f;
    [Min(0.05f)] public float AttackCooldown = 1f;
    [Min(0f)] public float Damage = 5f;
    [Min(0f)] public float ProjectileSpeed = 12f;
    [Min(0f)] public int DeathReward = 5;

    public UnitStats()
    {
    }

    public UnitStats(UnitStats source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        MaxHealth = source.MaxHealth;
        MoveSpeed = source.MoveSpeed;
        DetectionRadius = source.DetectionRadius;
        AttackCooldown = source.AttackCooldown;
        Damage = source.Damage;
        ProjectileSpeed = source.ProjectileSpeed;
        DeathReward = source.DeathReward;
    }

    public void IncreaseMaxHealth(float amount)
    {
        MaxHealth = Mathf.Max(1f, MaxHealth + amount);
    }

    public void IncreaseDamage(float amount)
    {
        Damage = Mathf.Max(0f, Damage + amount);
    }

    public void ReduceAttackCooldown(float amount)
    {
        AttackCooldown = Mathf.Max(0.05f, AttackCooldown - amount);
    }

    public void MultiplyDamage(float multiplier)
    {
        Damage = Mathf.Max(0f, Damage * Mathf.Max(0f, multiplier));
    }

    public void MultiplyMaxHealth(float multiplier)
    {
        MaxHealth = Mathf.Max(1f, MaxHealth * Mathf.Max(0f, multiplier));
    }
}
