using UnityEngine;

public class SoldierIdleState : SoldierStateBase
{
    private readonly ISquadMovementStateReader movementStateReader;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;
    private readonly SquadFollowSettings squadFollowSettings;
    private readonly SoldierMovingFormationService movingFormationService;
    private readonly UnitRotatorService unitRotatorService;

    public SoldierIdleState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        ISquadMovementStateReader movementStateReader,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        SquadFollowSettings squadFollowSettings,
        SoldierMovingFormationService movingFormationService,
        UnitRotatorService unitRotatorService)
        : base(model, modules, agentAnimationController)
    {
        this.movementStateReader = movementStateReader;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.squadFollowSettings = squadFollowSettings;
        this.movingFormationService = movingFormationService;
        this.unitRotatorService = unitRotatorService;
    }

    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Idle);
    }

    public override void Tick()
    {
        if (!Soldier.HasFormationAssignment)
        {
            return;
        }

        if (Soldier.HasValidTarget)
        {
            ChangeState<SoldierAttackState>();
            return;
        }

        if (movementStateReader.IsMoving)
        {
            ChangeState<SoldierMoveState>();
            return;
        }

        movingFormationService.Reset();
        Vector3 slotCenter = Soldier.GetAssignedSlotCenter(squadSlotPositionProvider);
        Vector3 delta = slotCenter - Soldier.Transform.position;
        delta.y = 0f;

        if (delta.magnitude > squadFollowSettings.SlotReachThreshold)
        {
            ChangeState<SoldierMoveState>();
            return;
        }

        Soldier.Transform.position = slotCenter;
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
