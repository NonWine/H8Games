using UnityEngine;

public class AttackRuntimeModel
{
    public float Damage;
    public Vector2 CooldownRange;
    public float CooldownRemaining;

    public AttackRuntimeModel(UnitStats stats)
    {
        Damage = stats.Damage;
        CooldownRange = stats.AttackCooldownRange;
        CooldownRemaining = 0f;
    }

    public float GetRandomizedCooldown()
    {
        float cooldown = Random.Range(CooldownRange.x, CooldownRange.y);
        UnityEngine.Debug.Log($"GetRandomizedCooldown: {cooldown}, Range: ({CooldownRange.x}, {CooldownRange.y})");
        return cooldown;
    }
}