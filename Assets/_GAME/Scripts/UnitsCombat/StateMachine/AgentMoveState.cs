using UnityEngine;

public class SoldierMoveState : SoldierStateBase
{
    private readonly ISquadMovementStateReader movementStateReader;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;
    private readonly SquadFollowSettings squadFollowSettings;
    private readonly SoldierMovingFormationService movingFormationService;
    private readonly UnitRotatorService unitRotatorService;
    private readonly ICombatTargetProvider combatTargetProvider;

    public SoldierMoveState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        ISquadMovementStateReader movementStateReader,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        SquadFollowSettings squadFollowSettings,
        SoldierMovingFormationService movingFormationService,
        UnitRotatorService unitRotatorService,
        ICombatTargetProvider combatTargetProvider)
        : base(model, modules, agentAnimationController)
    {
        this.movementStateReader = movementStateReader;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.squadFollowSettings = squadFollowSettings;
        this.movingFormationService = movingFormationService;
        this.unitRotatorService = unitRotatorService;
        this.combatTargetProvider = combatTargetProvider;
    }

    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Move);
    }

    public override void Tick()
    {
        if (!Soldier.HasFormationAssignment)
        {
            ChangeState<SoldierIdleState>();
            return;
        }

        if (combatTargetProvider.GetTarget() != null)
        {
            ChangeState<SoldierAttackState>();
            return;
        }

        Vector3 slotCenter = Soldier.GetAssignedSlotCenter(squadSlotPositionProvider);

        if (movementStateReader.IsMoving)
        {
            movingFormationService.Update(
                Soldier.Transform,
                Soldier.SquadRootView.transform,
                slotCenter,
                Time.deltaTime);
            return;
        }

        movingFormationService.Reset();
        Vector3 delta = slotCenter - Soldier.Transform.position;
        delta.y = 0f;
        if (delta.magnitude <= squadFollowSettings.SlotReachThreshold)
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
        Vector3 delta = slotCenter - Soldier.Transform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        if (distance <= squadFollowSettings.SlotReachThreshold)
        {
            Soldier.Transform.position = slotCenter;
            ChangeState<SoldierIdleState>();
            return;
        }

        float slowdownRadius = Mathf.Max(squadFollowSettings.SlotReachThreshold * 4f, squadFollowSettings.SlotReachThreshold + 0.01f);
        float speedFactor = distance < slowdownRadius
            ? Mathf.Lerp(0.35f, 1f, distance / slowdownRadius)
            : 1f;

        float step = squadFollowSettings.SoldierMoveSpeed * speedFactor * Time.deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(Soldier.Transform.position, slotCenter, step);
        Soldier.Transform.position = new Vector3(nextPosition.x, Soldier.Transform.position.y, nextPosition.z);
        unitRotatorService.RotateTowards(
            Soldier.Transform,
            delta.normalized,
            Time.deltaTime,
            squadFollowSettings.SoldierRotationSpeed);
    }
}
