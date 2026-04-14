using UnityEngine;
using Zenject;

public sealed class SoldierFollower : MonoBehaviour
{
    [SerializeField] private bool autoRegisterOnStart = true;

    private SquadFollowSettings settings;
    private SquadRoot squadRoot;
    private FormationSlot assignedSlot;

    public SoldierFormationState State { get; private set; } = SoldierFormationState.WaitingInFormation;
    public FormationSlot AssignedSlot => assignedSlot;

    [Inject]
    public void Construct(SquadFollowSettings settings, [InjectOptional] SquadRoot squadRoot)
    {
        this.settings = settings;
        this.squadRoot = squadRoot;
    }

    private void Start()
    {
        if (autoRegisterOnStart && squadRoot != null)
            squadRoot.RegisterSoldier(this);
    }

    private void Update()
    {
        if (settings == null || squadRoot == null || assignedSlot == null)
            return;

        Vector3 desiredPosition = squadRoot.GetSlotWorldPosition(assignedSlot);
        desiredPosition.y = transform.position.y;

        Vector3 delta = desiredPosition - transform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        if (distance <= settings.SlotReachThreshold)
        {
            transform.position = new Vector3(desiredPosition.x, transform.position.y, desiredPosition.z);
            State = squadRoot.IsMoving ? SoldierFormationState.FollowingFormation : SoldierFormationState.WaitingInFormation;
            RotateTowards(squadRoot.transform.forward, Time.deltaTime);
            return;
        }

        State = squadRoot.IsMoving ? SoldierFormationState.FollowingFormation : SoldierFormationState.MovingToSlot;

        float slowdownRadius = Mathf.Max(settings.SlotReachThreshold * 4f, settings.SlotReachThreshold + 0.01f);
        float speedFactor = distance < slowdownRadius
            ? Mathf.Lerp(0.35f, 1f, distance / slowdownRadius)
            : 1f;

        float step = settings.SoldierMoveSpeed * speedFactor * Time.deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, desiredPosition, step);
        transform.position = new Vector3(nextPosition.x, transform.position.y, nextPosition.z);
        RotateTowards(delta.normalized, Time.deltaTime);
    }

    private void OnDisable()
    {
        if (squadRoot == null)
            return;

        SquadRoot owner = squadRoot;
        squadRoot = null;
        assignedSlot = null;
        owner.UnregisterSoldier(this);
    }

    public void AssignSquad(SquadRoot squadRoot)
    {
        this.squadRoot = squadRoot;
    }

    public void AssignSlot(FormationSlot slot)
    {
        assignedSlot = slot;
    }

    public void ClearSquad(SquadRoot owner)
    {
        if (squadRoot != owner)
            return;

        assignedSlot = null;
        squadRoot = null;
        State = SoldierFormationState.WaitingInFormation;
    }

    public bool IsInAssignedSlot(float threshold)
    {
        if (squadRoot == null || assignedSlot == null)
            return false;

        Vector3 targetPosition = squadRoot.GetSlotWorldPosition(assignedSlot);
        Vector3 delta = targetPosition - transform.position;
        delta.y = 0f;
        return delta.sqrMagnitude <= threshold * threshold;
    }

    private void RotateTowards(Vector3 direction, float deltaTime)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            settings.SoldierRotationSpeed * deltaTime);
    }
}
