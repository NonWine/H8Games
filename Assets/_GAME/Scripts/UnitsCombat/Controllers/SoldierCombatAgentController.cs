using UnityEngine;

public class SoldierCombatAgentController : BaseCombatAgentController
{
    private readonly IEnemyGroupProvider currentEnemyGroupProvider;
    private readonly ISquadMovementStateReader movementStateReader;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;
    private readonly SquadFollowSettings squadFollowSettings;
    private readonly SoldierMovingFormationService movingFormationService;

    private SquadRootView squadRootView;
    private FormationSlot assignedSlot;

    public SoldierFormationState FormationState { get; private set; } = SoldierFormationState.WaitingInFormation;

    public SoldierCombatAgentController(
        BaseCombatAgentView baseCombatAgentView,
        ModulesFactoryCollection modulesFactoryCollection,
        SquadFollowSettings squadFollowSettings,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        ISquadMovementStateReader movementStateReader,
        IEnemyGroupProvider currentEnemyGroupProvider)
        : base(baseCombatAgentView, modulesFactoryCollection)
    {
        this.squadFollowSettings = squadFollowSettings;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        this.movementStateReader = movementStateReader;
        this.currentEnemyGroupProvider = currentEnemyGroupProvider;
        movingFormationService = new SoldierMovingFormationService(squadFollowSettings, baseCombatAgentView.GetInstanceID());
    }

    public override void Tick()
    {
        base.Tick();

        if (!IsAlive)
        {
            return;
        }

        EnemyGroupViewController currentGroup = currentEnemyGroupProvider.CurrentTargetGroup;
        var tracker = modules.TargetTracker;

        if (currentGroup == null || currentGroup.State != EnemyGroupState.Activated)
        {
            tracker.SetCurrentTarget(null);
            State = UnitState.Idle;
            UpdateFormation();
            return;
        }

        if (!tracker.IsCurrentTargetValid(currentGroup))
        {
            TryAcquireTarget(currentGroup);
        }
        else if (tracker.ShouldRetarget())
        {
            TryAcquireTarget(currentGroup);
        }

        if (!tracker.IsCurrentTargetValid(currentGroup))
        {
            tracker.SetCurrentTarget(null);
            State = UnitState.Idle;
            UpdateFormation();
            return;
        }

        tracker.RotateTowardsCurrentTarget(baseCombatAgentView.transform);
        State = UnitState.Attack;
        UpdateFormation();
    }

    public void AssignSquad(SquadRootView squadRootView)
    {
        this.squadRootView = squadRootView;
    }

    public void AssignSlot(FormationSlot slot)
    {
        assignedSlot = slot;
        movingFormationService.Reset();
        FormationState = SoldierFormationState.WaitingInFormation;
    }

    public void ClearSquad(SquadRootView owner)
    {
        if (squadRootView != owner)
        {
            return;
        }

        assignedSlot = null;
        squadRootView = null;
        movingFormationService.Reset();
        FormationState = SoldierFormationState.WaitingInFormation;
    }

    public bool IsInAssignedSlot(float threshold)
    {
        if (assignedSlot == null)
        {
            return false;
        }

        Vector3 targetPosition = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
        Vector3 delta = targetPosition - transform.position;
        delta.y = 0f;
        return delta.sqrMagnitude <= threshold * threshold;
    }

    public void ResetRunTimeState()
    {
        modules.ResetModules();
        State = UnitState.Idle;
        FormationState = SoldierFormationState.WaitingInFormation;
        movingFormationService.Reset();
    }

    private void UpdateFormation()
    {
        if (State == UnitState.Attack || State == UnitState.Dead || squadRootView == null || assignedSlot == null)
        {
            return;
        }

        Vector3 slotCenter = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
        slotCenter.y = transform.position.y;

        if (!movementStateReader.IsMoving)
        {
            movingFormationService.Reset();
            UpdateIdleFormation(slotCenter);
            return;
        }

        FormationState = movingFormationService.Update(
            transform,
            squadRootView.transform,
            slotCenter,
            Time.deltaTime);
        State = UnitState.Move;
    }

    private void UpdateIdleFormation(Vector3 slotCenter)
    {
        Vector3 delta = slotCenter - transform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        if (distance <= squadFollowSettings.SlotReachThreshold)
        {
            transform.position = slotCenter;
            FormationState = SoldierFormationState.WaitingInFormation;
            RotateTowards(squadRootView.transform.forward, Time.deltaTime, squadFollowSettings.SoldierRotationSpeed);
            State = UnitState.Idle;
            return;
        }

        FormationState = SoldierFormationState.MovingToSlot;
        State = UnitState.Move;

        float slowdownRadius = Mathf.Max(squadFollowSettings.SlotReachThreshold * 4f, squadFollowSettings.SlotReachThreshold + 0.01f);
        float speedFactor = distance < slowdownRadius
            ? Mathf.Lerp(0.35f, 1f, distance / slowdownRadius)
            : 1f;

        float step = squadFollowSettings.SoldierMoveSpeed * speedFactor * Time.deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, slotCenter, step);
        transform.position = new Vector3(nextPosition.x, transform.position.y, nextPosition.z);
        RotateTowards(delta.normalized, Time.deltaTime, squadFollowSettings.SoldierRotationSpeed);
    }

    private void TryAcquireTarget(EnemyGroupViewController currentGroup)
    {
        var tracker = modules.TargetTracker;

        if (currentGroup == null)
        {
            tracker.SetCurrentTarget(null);
            return;
        }

        ICombatTarget target = currentGroup.GetBestLivingEnemyTarget(baseCombatAgentView.transform.position, unitStats.ReservationPenalty);

        if (target == null)
        {
            tracker.SetCurrentTarget(null);
            return;
        }

        tracker.SetCurrentTarget(target);
        tracker.MarkRetargetWindow();
    }

    private void RotateTowards(Vector3 direction, float deltaTime, float rotationSpeed)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * deltaTime);
    }
}
