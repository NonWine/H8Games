using UnityEngine;

public class EnemyAttackState : EnemyStateBase
{
    private readonly BaseCombatAgentView combatView;
    private readonly UnitRotatorService unitRotatorService;
    private readonly AttackRuntimeModel attackData;
    private readonly EnemySortiePlanner sortiePlanner;
    private readonly EnemySortieConfig sortieConfig;

    public EnemyAttackState(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        BaseCombatAgentView combatView,
        UnitRotatorService unitRotatorService,
        AttackRuntimeModel attackData,
        EnemySortiePlanner sortiePlanner,
        EnemySortieConfig sortieConfig)
        : base(model, modules, agentAnimationController)
    {
        this.combatView = combatView;
        this.unitRotatorService = unitRotatorService;
        this.attackData = attackData;
        this.sortiePlanner = sortiePlanner;
        this.sortieConfig = sortieConfig;
    }

    public override void Enter()
    {
        attackData.CooldownRemaining = 0f;
        agentAnimationController.SetAnimationState(UnitState.Attack);
        sortiePlanner.BeginFight(Time.time);
    }

    public override void Tick()
    {
        if (!Enemy.HasValidTarget)
        {
            LeaveFight();
            return;
        }

        if (TryPlanSortie())
        {
            ChangeState<EnemySortieState>();
            return;
        }

        attackData.CooldownRemaining -= Time.deltaTime;

        if (attackData.CooldownRemaining <= 0f)
        {
            HandleAttack();
            attackData.CooldownRemaining = attackData.GetRandomizedCooldown();
        }

        unitRotatorService.RotateTowards(Enemy.Transform, Enemy.CurrentTarget.transform);
    }

    public override void Exit()
    {
        attackData.CooldownRemaining = attackData.GetRandomizedCooldown();
    }

    private void LeaveFight()
    {
        if (Enemy.IsAtPost(sortieConfig.PostReachThreshold))
        {
            ChangeState<EnemyIdleState>();
            return;
        }

        ChangeState<EnemyReturnState>();
    }

    private bool TryPlanSortie()
    {
        return sortiePlanner.TryPlanSortie(
            Time.time,
            Enemy.PostPosition,
            Enemy.Transform.position,
            Enemy.CurrentTarget.transform.position);
    }

    private void HandleAttack()
    {
        if (!Enemy.IsAlive || !Enemy.HasValidTarget)
        {
            return;
        }

        attackData.ShotsFired++;
        if (attackData.ShotsFired >= 8)
        {
            attackData.ShotsFired = 0;
            agentAnimationController.SetReloadTrigger();
            attackData.CooldownRemaining += 1.5f; // Extra pause for reload
            return;
        }

        ICombatTarget target = Enemy.CurrentTarget;
        agentAnimationController.SetAttackTrigger();
        modules.Attack.HandleAttack(
            target,
            combatView.AttackPoint,
            () => target.TakeDamage(unitStats.Damage, combatView.AttackPoint.position));
    }
}
