using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitAttackAgentHandler
{
    private const float MinAttackAnimationSpeedMultiplier = 0.92f;
    private const float MaxAttackAnimationSpeedMultiplier = 1.08f;
    private readonly AttackRuntimeModel attackData;
    private readonly ProjectileVisualSpawner projectileSpawner;

    public float AttackAnimationSpeedMultiplier { get; private set; } = 1f;

    public UnitAttackAgentHandler(AttackRuntimeModel attackData, ProjectileVisualSpawner projectileSpawner)
    {
        this.attackData = attackData;
        this.projectileSpawner = projectileSpawner;
        RandomizeAttackAnimationSpeed();
    }

    public float GetAttackAnimationSpeed(float animationCycleDuration)
    {
        float baseSpeed = animationCycleDuration / Mathf.Max(0.05f, attackData.Cooldown);
        return Mathf.Max(0.01f, baseSpeed * AttackAnimationSpeedMultiplier);
    }
    

    public void RandomizeAttackAnimationSpeed()
    {
        AttackAnimationSpeedMultiplier = Random.Range(
            MinAttackAnimationSpeedMultiplier,
            MaxAttackAnimationSpeedMultiplier);
    }
    
    public void HandleAttack(ICombatTarget target, Transform AttackPointStart, Action Hit)
    {
        projectileSpawner.Spawn(AttackPointStart, target.transform, 80f, Hit);
    }
}
