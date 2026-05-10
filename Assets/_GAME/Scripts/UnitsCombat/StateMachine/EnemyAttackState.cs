using UnityEngine;

public class EnemyAttackState : EnemyStateBase
{
    private readonly BaseCombatAgentView combatView;
    private readonly UnitRotatorService unitRotatorService;
    private readonly AttackRuntimeModel attackData;

    public EnemyAttackState(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        BaseCombatAgentView combatView,
        UnitRotatorService unitRotatorService,
        AttackRuntimeModel attackData)
        : base(model, modules, agentAnimationController)
    {
        this.combatView = combatView;
        this.unitRotatorService = unitRotatorService;
        this.attackData = attackData;
    }

    public override void Enter()
    {
        attackData.CooldownRemaining = 0f;
        agentAnimationController.SetAnimationState(UnitState.Attack);

    }

    public override void Tick()
    {
        if (!Enemy.HasValidTarget)
        {
            ChangeState<EnemyIdleState>();
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

    private void HandleAttack()
    {
        if (!Enemy.IsAlive || !Enemy.HasValidTarget)
        {
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
