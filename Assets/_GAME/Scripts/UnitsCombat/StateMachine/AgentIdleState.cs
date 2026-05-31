using UnityEngine;

public class SoldierIdleState : SoldierStateBase
{
    private readonly ISquadMovementStateReader movementStateReader;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;
    private readonly SquadFollowSettings squadFollowSettings;
    private readonly ISoldierFormationMover formationMover;
    private readonly UnitRotatorService unitRotatorService;

    public SoldierIdleState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        ISquadMovementStateReader movementStateReader,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        SquadFollowSettings squadFollowSettings,
        ISoldierFormationMover formationMover,
        UnitRotatorService unitRotatorService)
        : base(model, modules, agentAnimationController)
    {
        this.movementStateReader = movementStateReader;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.squadFollowSettings = squadFollowSettings;
        this.formationMover = formationMover;
        this.unitRotatorService = unitRotatorService;
    }

    public override void Enter()
    {
        formationMover.Stop();
        agentAnimationController.SetAnimationState(UnitState.Idle);
    }

    public override void Tick()
    {
        if (Soldier.HasValidTarget)
        {
            ChangeState<SoldierAttackState>();
            return;
        }

        if (!Soldier.HasFormationAssignment)
        {
            return;
        }
        
        if (movementStateReader.IsMoving)
        {
            ChangeState<SoldierMoveState>();
            return;
        }

        Vector3 slotCenter = Soldier.GetAssignedSlotCenter(squadSlotPositionProvider);
        Vector3 delta = slotCenter - Soldier.Transform.position;
        delta.y = 0f;
        float distance = delta.magnitude;

        if (distance > squadFollowSettings.SlotReachThreshold)
        {
            ChangeState<SoldierMoveState>();
            return;
        }

        formationMover.Stop();
        unitRotatorService.RotateTowards(
            Soldier.Transform,
            Soldier.SquadRootView.transform.forward,
            Time.deltaTime,
            squadFollowSettings.SoldierRotationSpeed);
    }

    public override void Exit()
    {
    }
}
