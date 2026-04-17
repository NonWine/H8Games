using UnityEngine;
using Zenject;

public class SoldierFollower : MonoBehaviour
{
    private SquadFollowSettings settings;
    private ISquadSlotPositionProvider squadSlotPositionProvider;
    private ISquadMovementStateReader movementStateReader;
    private SquadRootView squadRootView;
    private FormationSlot assignedSlot;
    private SoldierMovingFormationService movingFormationService;

    public SoldierFormationState State { get; private set; } = SoldierFormationState.WaitingInFormation;
    public FormationSlot AssignedSlot => assignedSlot;

    [Inject]
    public void Construct(
        SquadFollowSettings settings,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        ISquadMovementStateReader movementStateReader,
        SquadRootView squadRootView)
    {
        this.settings = settings;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.movementStateReader = movementStateReader;
        this.squadRootView = squadRootView;
        movingFormationService = new SoldierMovingFormationService(settings, GetInstanceID());
    }

    public void UpdateFormation()
    {
        if (assignedSlot == null)
            return;

        Vector3 slotCenter = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
        slotCenter.y = transform.position.y;

        if (!movementStateReader.IsMoving)
        {
            movingFormationService.Reset();
            UpdateIdleFormation(slotCenter);
            return;
        }

        State = movingFormationService.Update(transform, squadRootView.transform, slotCenter, Time.deltaTime);
    }

    private void OnDisable()
    {
        squadRootView = null;
        assignedSlot = null;
        movingFormationService.Reset();
    }

    public void AssignSquad(SquadRootView squadRootView)
    {
        this.squadRootView = squadRootView;
    }

    public void AssignSlot(FormationSlot slot)
    {
        assignedSlot = slot;
        movingFormationService.Reset();
    }

    public void ClearSquad(SquadRootView owner)
    {
        if (squadRootView != owner)
            return;

        assignedSlot = null;
        squadRootView = null;
        movingFormationService.Reset();
        State = SoldierFormationState.WaitingInFormation;
    }

    public bool IsInAssignedSlot(float threshold)
    {
        if (assignedSlot == null)
            return false;

        Vector3 targetPosition = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
        Vector3 delta = targetPosition - transform.position;
        delta.y = 0f;
        return delta.sqrMagnitude <= threshold * threshold;
    }

    private void RotateTowards(Vector3 direction, float deltaTime, float rotationSpeed)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * deltaTime);
    }

    private void UpdateIdleFormation(Vector3 slotCenter)
    {
        Vector3 delta = slotCenter - transform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        if (distance <= settings.SlotReachThreshold)
        {
            transform.position = slotCenter;
            State = SoldierFormationState.WaitingInFormation;
            RotateTowards(squadRootView.transform.forward, Time.deltaTime, settings.SoldierRotationSpeed);
            return;
        }

        State = SoldierFormationState.MovingToSlot;

        float slowdownRadius = Mathf.Max(settings.SlotReachThreshold * 4f, settings.SlotReachThreshold + 0.01f);
        float speedFactor = distance < slowdownRadius
            ? Mathf.Lerp(0.35f, 1f, distance / slowdownRadius)
            : 1f;

        float step = settings.SoldierMoveSpeed * speedFactor * Time.deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, slotCenter, step);
        transform.position = new Vector3(nextPosition.x, transform.position.y, nextPosition.z);
        RotateTowards(delta.normalized, Time.deltaTime, settings.SoldierRotationSpeed);
    }
}
