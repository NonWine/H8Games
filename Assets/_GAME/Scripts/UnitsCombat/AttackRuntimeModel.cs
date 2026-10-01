using UnityEngine;

public class AttackRuntimeModel
{
    public float Damage;
    public Vector2 CooldownRange;
    public float CooldownRemaining;
    public int ShotsFired;

    public AttackRuntimeModel(UnitStats stats)
    {
        Damage = stats.Damage;
        CooldownRange = stats.AttackCooldownRange;
        CooldownRemaining = 0f;
        ShotsFired = 0;
    }

    public float GetRandomizedCooldown() => Random.Range(CooldownRange.x, CooldownRange.y);
}