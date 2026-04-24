using System;
using UnityEngine;

public class CombatAnimationPresenter : ICombatTickModule, IResetModule, IDisposeModule
{
    private readonly BaseCombatUnitView view;
    private readonly UnitAttackAgentHandler attack;
    private readonly UnitHealthHandler health;
    private readonly CombatTargetTracker targetTracker;
    private readonly ProjectileVisualSpawner projectileSpawner;
    private readonly float projectileSpeed;
    private readonly Animator animator;

    private UnitState currentState = UnitState.Idle;

    public CombatAnimationPresenter(
        BaseCombatUnitView view,
        UnitAttackAgentHandler attack,
        UnitHealthHandler health,
        CombatTargetTracker targetTracker,
        ProjectileVisualSpawner projectileSpawner,
        float projectileSpeed)
    {
        this.view = view;
        this.attack = attack;
        this.health = health;
        this.targetTracker = targetTracker;
        this.projectileSpawner = projectileSpawner;
        this.projectileSpeed = projectileSpeed;
        animator = view.Animator;

        view.AttackAnimationEvents.AttackTriggered += HandleAttackAnimationTriggered;
    }

    public void Tick(UnitState state, float deltaTime)
    {
        currentState = state;

        animator.speed = state == UnitState.Attack
            ? attack.GetAttackAnimationSpeed(view.AttackAnimationCycleDuration)
            : 1f;

        switch (state)
        {
            case UnitState.Idle:
                animator.SetInteger("State", 0);
                break;
            case UnitState.Move:
                animator.SetInteger("State", 1);
                break;
            case UnitState.Attack:
                animator.SetInteger("State", 2);
                break;
            case UnitState.Dead:
                animator.SetInteger("State", 3);
                break;
        }
    }

    public void Reset()
    {
        currentState = UnitState.Idle;
        animator.speed = 1f;
        animator.SetInteger("State", 0);
    }

    public void Dispose()
    {
        view.AttackAnimationEvents.AttackTriggered -= HandleAttackAnimationTriggered;
    }

    private void HandleAttackAnimationTriggered()
    {
        if (!health.IsAlive || currentState != UnitState.Attack || !targetTracker.IsCurrentTargetValid())
        {
            return;
        }

        ICombatTarget currentTarget = targetTracker.CurrentTarget;
        Vector3 attackOrigin = view.AttackPoint.position;

        if (!projectileSpawner.Spawn(
                view.AttackPoint,
                currentTarget.transform,
                projectileSpeed,
                () => attack.ApplyDamage(currentTarget, attackOrigin)))
        {
            attack.ApplyDamage(currentTarget, attackOrigin);
        }
    }
}
