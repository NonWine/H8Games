public class AgentAttackState : AgentStateBase
{
    private readonly ITargetTrackerHandler targetTracker;
   public UnitAttackAnimationEventRelay AttackAnimationEvents { get; private set; }


    public AgentAttackState(ITargetTrackerHandler targetTracker)
    {
        this.targetTracker = targetTracker;
    }

    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Attack);
        AttackAnimationEvents.AttackTriggered += HandleAttack;
    }

    public override void Exit()
    {
        AttackAnimationEvents.AttackTriggered -= HandleAttack;
    }
    
    private void HandleAttack()
    {
        if (!modules.Health.IsAlive || !targetTracker.IsCurrentTargetValid())
        {
            return;
        }

        ICombatTarget target = targetTracker.CurrentTarget;

        modules.Attack.HandleAttack(
            target,
            baseCombatAgentView.AttackPoint,
            () => target.TakeDamage(unitStats.Damage, baseCombatAgentView.AttackPoint.position));
    }

}
