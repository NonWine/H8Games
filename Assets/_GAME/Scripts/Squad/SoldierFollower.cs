using UnityEngine;
using Zenject;

public class SoldierFollower : MonoBehaviour
{
    [SerializeField] private bool autoRegisterOnStart = true;

    private SquadFollowSettings settings;
    private ISquadSlotPositionProvider squadSlotPositionProvider;
    private ISoldierFollowerRegistratorProvider registrator;
    private SquadRootView _squadRootView;
    private FormationSlot assignedSlot;

    public SoldierFormationState State { get; private set; } = SoldierFormationState.WaitingInFormation;
    public FormationSlot AssignedSlot => assignedSlot;

    [Inject]
    public void Construct(
        SquadFollowSettings settings,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        ISoldierFollowerRegistratorProvider registrator,
        [InjectOptional] SquadRootView squadRootView)
    {
        this.settings = settings;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.registrator = registrator;
        this._squadRootView = squadRootView;
    }

    private void Start()
    {
        if (autoRegisterOnStart)
            registrator?.RegisterSoldier(this);
    }

    private void Update()
    {
        if (settings == null || squadSlotPositionProvider == null || assignedSlot == null)
            return;

        Vector3 desiredPosition = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
        desiredPosition.y = transform.position.y;

        Vector3 delta = desiredPosition - transform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        if (distance <= settings.SlotReachThreshold)
        {
            transform.position = new Vector3(desiredPosition.x, transform.position.y, desiredPosition.z);
            State = SoldierFormationState.WaitingInFormation;
            Vector3 forward = _squadRootView != null ? _squadRootView.transform.forward : Vector3.forward;
            RotateTowards(forward, Time.deltaTime);
            return;
        }

        State = SoldierFormationState.MovingToSlot;

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
        if (registrator != null)
            registrator.UnregisterSoldier(this);

        _squadRootView = null;
        assignedSlot = null;
        State = SoldierFormationState.WaitingInFormation;
    }

    public void AssignSquad(SquadRootView squadRootView)
    {
        this._squadRootView = squadRootView;
    }

    public void AssignSlot(FormationSlot slot)
    {
        assignedSlot = slot;
    }

    public void ClearSquad(SquadRootView owner)
    {
        if (_squadRootView != owner)
            return;

        assignedSlot = null;
        _squadRootView = null;
        State = SoldierFormationState.WaitingInFormation;
    }

    public bool IsInAssignedSlot(float threshold)
    {
        if (assignedSlot == null || squadSlotPositionProvider == null)
            return false;

        Vector3 targetPosition = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
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
