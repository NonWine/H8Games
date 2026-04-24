using UnityEngine;

public class SoldierFormationHandler 
{
    private readonly ISquadMovementStateReader movementStateReader;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;
    private readonly SquadFollowSettings squadFollowSettings;
    private readonly SoldierMovingFormationService movingFormationService;

    public SoldierFormationState FormationState { get; private set; } = SoldierFormationState.WaitingInFormation;

    public SoldierFormationHandler(
        ISquadMovementStateReader movementStateReader,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        SquadFollowSettings squadFollowSettings,
        int seed)
    {
        this.movementStateReader = movementStateReader;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.squadFollowSettings = squadFollowSettings;
        movingFormationService = new SoldierMovingFormationService(squadFollowSettings, seed);
    }

    public void Reset()
    {
        movingFormationService.Reset();
        FormationState = SoldierFormationState.WaitingInFormation;
    }

    public UnitState UpdateFormation(
        Transform soldierTransform,
        float deltaTime,
        UnitState state,
        SquadRootView squadRootView,
        FormationSlot assignedSlot)
    {
        if (state == UnitState.Attack || state == UnitState.Dead || squadRootView == null || assignedSlot == null)
        {
            return state;
        }

        Vector3 slotCenter = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
        slotCenter.y = soldierTransform.position.y;

        if (!movementStateReader.IsMoving)
        {
            movingFormationService.Reset();
            return UpdateIdleFormation(soldierTransform, slotCenter, deltaTime, squadRootView);
        }

        FormationState = movingFormationService.Update(
            soldierTransform,
            squadRootView.transform,
            slotCenter,
            deltaTime);
        return UnitState.Move;
    }

    private UnitState UpdateIdleFormation(
        Transform soldierTransform,
        Vector3 slotCenter,
        float deltaTime,
        SquadRootView squadRootView)
    {
        Vector3 delta = slotCenter - soldierTransform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        if (distance <= squadFollowSettings.SlotReachThreshold)
        {
            soldierTransform.position = slotCenter;
            FormationState = SoldierFormationState.WaitingInFormation;
            RotateTowards(soldierTransform, squadRootView.transform.forward, deltaTime, squadFollowSettings.SoldierRotationSpeed);
            return UnitState.Idle;
        }

        FormationState = SoldierFormationState.MovingToSlot;

        float slowdownRadius = Mathf.Max(squadFollowSettings.SlotReachThreshold * 4f, squadFollowSettings.SlotReachThreshold + 0.01f);
        float speedFactor = distance < slowdownRadius
            ? Mathf.Lerp(0.35f, 1f, distance / slowdownRadius)
            : 1f;

        float step = squadFollowSettings.SoldierMoveSpeed * speedFactor * deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(soldierTransform.position, slotCenter, step);
        soldierTransform.position = new Vector3(nextPosition.x, soldierTransform.position.y, nextPosition.z);
        RotateTowards(soldierTransform, delta.normalized, deltaTime, squadFollowSettings.SoldierRotationSpeed);
        return UnitState.Move;
    }

    private static void RotateTowards(Transform soldierTransform, Vector3 direction, float deltaTime, float rotationSpeed)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        soldierTransform.rotation = Quaternion.RotateTowards(
            soldierTransform.rotation,
            targetRotation,
            rotationSpeed * deltaTime);
    }
}
