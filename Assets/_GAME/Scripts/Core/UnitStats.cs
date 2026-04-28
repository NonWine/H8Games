using System;
using UnityEngine;

[Serializable]
public class UnitStats
{
    [Min(1f)] public float MaxHealth = 30f;
    [Min(0f)] public float MoveSpeed = 3.5f;
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
        AttackCooldown = source.AttackCooldown;
        Damage = source.Damage;
        ProjectileSpeed = source.ProjectileSpeed;
        DeathReward = source.DeathReward;
    }

    public UnitStats Clone()
    {
        return new UnitStats(this);
    }
}
