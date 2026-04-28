public class EnemyAttackState : EnemyStateBase
{
    private readonly BaseCombatAgentView combatView;
    private readonly UnitRotatorService unitRotatorService;

    public EnemyAttackState(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        BaseCombatAgentView combatView,
        UnitRotatorService unitRotatorService)
        : base(model, modules, agentAnimationController)
    {
        this.combatView = combatView;
        this.unitRotatorService = unitRotatorService;
    }

    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Attack);
        combatView.AttackAnimationEvents.AttackTriggered += HandleAttack;
    }

    public override void Tick()
    {
        if (!Enemy.HasValidTarget)
        {
            ChangeState<EnemyIdleState>();
            return;
        }

        unitRotatorService.RotateTowards(Enemy.Transform, Enemy.CurrentTarget.transform);
    }

    public override void Exit()
    {
        combatView.AttackAnimationEvents.AttackTriggered -= HandleAttack;
    }

    private void HandleAttack()
    {
        if (!Enemy.IsAlive || !Enemy.HasValidTarget)
        {
            return;
        }

        ICombatTarget target = Enemy.CurrentTarget;

        modules.Attack.HandleAttack(
            target,
            combatView.AttackPoint,
            () => target.TakeDamage(unitStats.Damage, combatView.AttackPoint.position));
    }
}
