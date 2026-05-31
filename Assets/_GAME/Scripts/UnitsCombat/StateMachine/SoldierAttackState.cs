public class SoldierAttackState : SoldierStateBase
{
    private readonly BaseCombatAgentView combatView;
    private readonly UnitRotatorService unitRotatorService;
    private readonly ISoldierFormationMover formationMover;
    private readonly AttackRuntimeModel attackData;

    public SoldierAttackState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        BaseCombatAgentView combatView,
        UnitRotatorService unitRotatorService,
        ISoldierFormationMover formationMover,
        AttackRuntimeModel attackData)
        : base(model, modules, agentAnimationController)
    {
        this.combatView = combatView;
        this.unitRotatorService = unitRotatorService;
        this.formationMover = formationMover;
        this.attackData = attackData;
    }

    public override void Enter()
    {
        formationMover.Stop();
        agentAnimationController.SetAnimationState(UnitState.Attack);
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

        attackData.CooldownRemaining -= UnityEngine.Time.deltaTime;

        if (attackData.CooldownRemaining <= 0f)
        {
            HandleAttack();
            attackData.CooldownRemaining = attackData.GetRandomizedCooldown();
        }

        unitRotatorService.RotateTowards(Soldier.Transform, Soldier.CurrentTarget.transform);
    }

    public override void Exit()
    {
    }

    private void HandleAttack()
    {
        if (!Soldier.IsAlive || !Soldier.HasValidTarget)
        {
            return;
        }

        ICombatTarget target = Soldier.CurrentTarget;
        agentAnimationController.SetAttackTrigger();
        modules.Attack.HandleAttack(
            target,
            combatView.AttackPoint,
            () => target.TakeDamage(unitStats.Damage, combatView.AttackPoint.position));
    }
}
