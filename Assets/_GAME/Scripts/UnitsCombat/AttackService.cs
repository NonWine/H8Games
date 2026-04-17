using UnityEngine;

public class UnitAttackAgentHandler
{
    private const float MinAttackAnimationSpeedMultiplier = 0.92f;
    private const float MaxAttackAnimationSpeedMultiplier = 1.08f;
    private readonly AttackRuntimeModel attackData;

    public float AttackAnimationSpeedMultiplier { get; private set; } = 1f;

    public UnitAttackAgentHandler(AttackRuntimeModel attackData)
    {
        this.attackData = attackData;
        RandomizeAttackAnimationSpeed();
    }

    public float GetAttackAnimationSpeed(float animationCycleDuration)
    {
        float baseSpeed = animationCycleDuration / Mathf.Max(0.05f, attackData.Cooldown);
        return Mathf.Max(0.01f, baseSpeed * AttackAnimationSpeedMultiplier);
    }

    public void ApplyDamage(IDamageable target, Vector3 attackOrigin)
    {
        if (target == null || !target.IsAlive)
        {
            return;
        }

        target.GetDamage(Mathf.Max(0f, attackData.Damage), attackOrigin);
    }

    public void RandomizeAttackAnimationSpeed()
    {
        AttackAnimationSpeedMultiplier = Random.Range(
            MinAttackAnimationSpeedMultiplier,
            MaxAttackAnimationSpeedMultiplier);
    }
}
