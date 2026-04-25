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

    public override void Tick()
    {
        if (!Soldier.HasValidTarget)
        {
            ChangeState<SoldierIdleState>();
        }
    }

    public override void Exit()
    {
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered -= HandleAttack;
    }
    
    private void HandleAttack()
    {
        if (!Soldier.IsAlive || !Soldier.HasValidTarget)
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
