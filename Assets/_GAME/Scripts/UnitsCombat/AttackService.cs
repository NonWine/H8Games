using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitAttackAgentHandler : IAttackModule
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
    

    public void RandomizeAttackAnimationSpeed()
    {
        AttackAnimationSpeedMultiplier = Random.Range(
            MinAttackAnimationSpeedMultiplier,
            MaxAttackAnimationSpeedMultiplier);
    }
    
    public void HandleAttack(ICombatTarget target, Transform attackPoint, Action onHit)
    {
        projectileSpawner.Spawn(attackPoint, target.transform, 80f, onHit);
    }
}
