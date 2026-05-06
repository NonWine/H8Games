using UnityEngine;

public class SoldierMoveState : SoldierStateBase
{
    private readonly ISquadMovementStateReader movementStateReader;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;
    private readonly SquadFollowSettings squadFollowSettings;
    private readonly ISoldierFormationMover formationMover;

    public SoldierMoveState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        ISquadMovementStateReader movementStateReader,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        SquadFollowSettings squadFollowSettings,
        ISoldierFormationMover formationMover)
        : base(model, modules, agentAnimationController)
    {
        this.movementStateReader = movementStateReader;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.squadFollowSettings = squadFollowSettings;
        this.formationMover = formationMover;
    }

    public override void Enter()
    {
        formationMover.Reset();
        agentAnimationController.SetAnimationState(UnitState.Move);
    }

    public override void Tick()
    {
        if (!Soldier.HasFormationAssignment)
        {
            ChangeState<SoldierIdleState>();
            return;
        }

        Vector3 slotCenter = Soldier.GetAssignedSlotCenter(squadSlotPositionProvider);

        if (movementStateReader.IsMoving)
        {
            formationMover.MoveToSlot(
                Soldier.SquadRootView.transform,
                slotCenter,
                true,
                Time.deltaTime);
            return;
        }

        if (formationMover.IsAt(slotCenter, squadFollowSettings.SlotReachThreshold))
        {
            ChangeState<SoldierIdleState>();
            return;
        }

        UpdateIdleSlotMovement(slotCenter);
    }

    public override void Exit()
    {
    }

    private void UpdateIdleSlotMovement(Vector3 slotCenter)
    {
        formationMover.MoveToSlot(
            Soldier.SquadRootView.transform,
            slotCenter,
            false,
            Time.deltaTime);
    }
}
