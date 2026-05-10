using System;
using UnityEngine;

[Serializable]
public class UnitStats
{
    [Min(1f)] public float MaxHealth = 30f;
    [Min(0f)] public float MoveSpeed = 3.5f;
    public Vector2 AttackCooldownRange = new Vector2(0.8f, 1.5f);
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
        AttackCooldownRange = source.AttackCooldownRange;
        Damage = source.Damage;
        ProjectileSpeed = source.ProjectileSpeed;
        DeathReward = source.DeathReward;
    }

    public UnitStats Clone()
    {
        return new UnitStats(this);
    }
}
