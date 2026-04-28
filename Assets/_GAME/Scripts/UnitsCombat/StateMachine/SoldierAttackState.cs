public class SoldierAttackState : SoldierStateBase
{
    private readonly BaseCombatAgentView combatView;
    private readonly UnitRotatorService unitRotatorService;

    public SoldierAttackState(
        SoldierRuntimeModel model,
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
        if (!Soldier.HasValidTarget)
        {
            if (Soldier.HasFormationAssignment)
                ChangeState<SoldierMoveState>();
            else
                ChangeState<SoldierIdleState>();

            return;
        }

        unitRotatorService.RotateTowards(Soldier.Transform, Soldier.CurrentTarget.transform);
    }

    public override void Exit()
    {
        combatView.AttackAnimationEvents.AttackTriggered -= HandleAttack;
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
            combatView.AttackPoint,
            () => target.TakeDamage(unitStats.Damage, combatView.AttackPoint.position));
    }
}
