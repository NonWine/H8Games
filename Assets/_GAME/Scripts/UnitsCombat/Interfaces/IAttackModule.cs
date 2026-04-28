using System;
using UnityEngine;

public interface IAttackModule
{
    float AttackAnimationSpeedMultiplier { get; }

    void HandleAttack(ICombatTarget target, Transform attackPoint, Action onHit);
}
