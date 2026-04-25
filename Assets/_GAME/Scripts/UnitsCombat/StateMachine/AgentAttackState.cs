public class SoldierAttackState : SoldierStateBase
{
    public SoldierAttackState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController)
        : base(model, modules, agentAnimationController)
    {
    }

    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Attack);
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered += HandleAttack;
    }

    public override void Exit()
    {
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered -= HandleAttack;
    }
    
    private void HandleAttack()
    {
        if (!modules.Health.IsAlive || !Soldier.HasValidTarget)
        {
            return;
        }

        ICombatTarget target = Soldier.CurrentTarget;

        modules.Attack.HandleAttack(
            target,
            baseCombatAgentView.AttackPoint,
            () => target.TakeDamage(unitStats.Damage, baseCombatAgentView.AttackPoint.position));
    }
}

public class EnemyAttackState : EnemyStateBase
{
    public EnemyAttackState(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController)
        : base(model, modules, agentAnimationController)
    {
    }

    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Attack);
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered += HandleAttack;
    }

    public override void Exit()
    {
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered -= HandleAttack;
    }
    
    private void HandleAttack()
    {
        if (!modules.Health.IsAlive || !Enemy.HasValidTarget)
        {
            return;
        }

        ICombatTarget target = Enemy.CurrentTarget;

        modules.Attack.HandleAttack(
            target,
            baseCombatAgentView.AttackPoint,
            () => target.TakeDamage(unitStats.Damage, baseCombatAgentView.AttackPoint.position));
    }
}
