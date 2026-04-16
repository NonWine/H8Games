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

    [SerializeField, Min(0f)] public float ReservationPenalty = 3f;
    [SerializeField, Min(0.05f)] public float RetargetInterval = 0.35f;
    [SerializeField, Min(0.05f)] public float TargetLockDuration = 0.35f;

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
        ReservationPenalty = source.ReservationPenalty;
        RetargetInterval = source.RetargetInterval;
        TargetLockDuration = source.TargetLockDuration;
    }

    public UnitStats Clone()
    {
        return new UnitStats(this);
    }
}