using System.Collections.Generic;
using UnityEngine;
using Zenject;

public sealed class SquadRoot : MonoBehaviour
{
    public event System.Action MovementTargetReached;

    public enum MovementMode
    {
        Forward = 0,
        TargetPoint = 1
    }

    [Header("Capacity")]
    [Min(0)]
    [SerializeField] private int initialCapacity = 6;

    [Header("Movement")]
    [SerializeField] private MovementMode movementMode = MovementMode.TargetPoint;
    [SerializeField] private Transform testTargetPoint;
    [SerializeField] private Vector3 forwardDirection = Vector3.forward;
    [Min(0.01f)] [SerializeField] private float targetReachThreshold = 0.1f;
    [SerializeField] private bool stopOnTargetReached = true;

    private readonly List<SoldierFollower> soldiers = new();
    private readonly List<FormationSlot> slots = new();

    private FormationLayoutService formationLayoutService;
    private SquadFollowSettings settings;
    private int capacity;
    private bool hasStaticTargetPoint;
    private Vector3 staticTargetPoint;
    private bool movementTargetReachedRaised;
    private Vector3 homePosition;
    private Quaternion homeRotation;

    public bool IsMoving => State == SquadRootState.MovingToZone || State == SquadRootState.ReturningToRegroup;
    public int Capacity => capacity;
    public int SoldierCount => soldiers.Count;
    public bool HasFreeSlot => soldiers.Count < capacity;
    public IReadOnlyList<FormationSlot> Slots => slots;
    public Vector3 HomePosition => homePosition;
    public Quaternion HomeRotation => homeRotation;
    public SquadRootState State { get; private set; } = SquadRootState.WaitingStart;
    public bool IsFormationSettled
    {
        get
        {
            float threshold = settings != null ? settings.SlotReachThreshold * 1.5f : 0.2f;

            for (int i = 0; i < soldiers.Count; i++)
            {
                SoldierFollower soldier = soldiers[i];
                if (soldier == null || !soldier.gameObject.activeInHierarchy)
                    continue;

                if (!soldier.IsInAssignedSlot(threshold))
                    return false;
            }

            return true;
        }
    }

    [Inject]
    public void Construct(
        FormationLayoutService formationLayoutService,
        SquadFollowSettings settings)
    {
        this.formationLayoutService = formationLayoutService;
        this.settings = settings;
    }

    private void Awake()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
        capacity = Mathf.Max(0, initialCapacity);
        RebuildFormation();
    }

    private void Update()
    {
        if (!IsMoving || settings == null)
            return;

        switch (movementMode)
        {
            case MovementMode.Forward:
                MoveForward(Time.deltaTime);
                break;
            case MovementMode.TargetPoint:
                MoveToTarget(Time.deltaTime);
                break;
        }
    }

    public bool RegisterSoldier(SoldierFollower soldier)
    {
        if (soldier == null)
            return false;

        PruneSoldiers();

        if (soldiers.Contains(soldier) || !HasFreeSlot)
            return false;

        soldiers.Add(soldier);
        soldier.AssignSquad(this);
        RebuildFormation();
        return true;
    }

    public void UnregisterSoldier(SoldierFollower soldier)
    {
        if (soldier == null)
            return;

        if (!soldiers.Remove(soldier))
            return;

        soldier.ClearSquad(this);
        RebuildFormation();
    }

    public void IncreaseCapacity(int amount)
    {
        if (amount <= 0)
            return;

        capacity += amount;
        RebuildFormation();
    }

    public void RebuildFormation()
    {
        PruneSoldiers();
        slots.Clear();

        List<Vector3> offsets = formationLayoutService.CalculateLocalOffsets(capacity);
        for (int i = 0; i < offsets.Count; i++)
        {
            slots.Add(new FormationSlot(i, offsets[i]));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            SoldierFollower soldier = i < soldiers.Count ? soldiers[i] : null;
            FormationSlot slot = slots[i];
            slot.AssignedSoldier = soldier;

            if (soldier != null)
            {
                soldier.AssignSquad(this);
                soldier.AssignSlot(slot);
            }
        }
    }

    public Vector3 GetSlotWorldPosition(FormationSlot slot)
    {
        return transform.TransformPoint(slot.LocalOffset);
    }

    public Vector3 GetSlotWorldPosition(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return transform.position;

        return GetSlotWorldPosition(slots[slotIndex]);
    }

    public void StartSearchingForNextZone()
    {
        State = SquadRootState.SearchingNextZone;
    }

    public void StopMovement()
    {
        State = SquadRootState.WaitingStart;
    }

    public void StopForEncounter()
    {
        State = SquadRootState.FightingZone;
    }

    public void ResumeAfterEncounter()
    {
        State = SquadRootState.SearchingNextZone;
    }

    public void MoveToZone(Transform target)
    {
        SetMovementTarget(target);
        movementMode = MovementMode.TargetPoint;
        State = SquadRootState.MovingToZone;
    }

    public void MoveToZone(Vector3 worldPosition)
    {
        SetMovementTarget(worldPosition);
        movementMode = MovementMode.TargetPoint;
        State = SquadRootState.MovingToZone;
    }

    public void StartReturningToRegroup(Transform target)
    {
        SetMovementTarget(target);
        movementMode = MovementMode.TargetPoint;
        State = SquadRootState.ReturningToRegroup;
    }

    public void StartReturningToRegroup(Vector3 worldPosition)
    {
        SetMovementTarget(worldPosition);
        movementMode = MovementMode.TargetPoint;
        State = SquadRootState.ReturningToRegroup;
    }

    public void EnterIdleInPreparation()
    {
        transform.position = homePosition;
        transform.rotation = homeRotation;
        hasStaticTargetPoint = false;
        testTargetPoint = null;
        movementTargetReachedRaised = false;
        RebuildFormation();
        State = SquadRootState.IdleInPreparation;
    }

    public void MarkDefeated()
    {
        State = SquadRootState.Defeated;
    }

    public void SetMovementTarget(Transform target)
    {
        testTargetPoint = target;
        hasStaticTargetPoint = false;
        movementTargetReachedRaised = false;
    }

    public void SetMovementTarget(Vector3 worldPosition)
    {
        staticTargetPoint = worldPosition;
        hasStaticTargetPoint = true;
        testTargetPoint = null;
        movementTargetReachedRaised = false;
    }

    public void StartMoveTo(Transform target)
    {
        MoveToZone(target);
    }

    public void StartMoveTo(Vector3 worldPosition)
    {
        MoveToZone(worldPosition);
    }

    public void SetForwardDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        forwardDirection = direction.normalized;
    }

    private void MoveForward(float deltaTime)
    {
        Vector3 direction = forwardDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            direction = transform.forward;

        direction = direction.normalized;
        transform.position += direction * settings.RootMoveSpeed * deltaTime;
        RotateTowards(direction, deltaTime);
    }

    private void MoveToTarget(float deltaTime)
    {
        if (!TryGetTargetPosition(out Vector3 targetPosition))
            return;

        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= targetReachThreshold * targetReachThreshold)
        {
            RaiseMovementTargetReached();

            return;
        }

        Vector3 direction = toTarget.normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, settings.RootMoveSpeed * deltaTime);
        RotateTowards(direction, deltaTime);
    }

    private void RotateTowards(Vector3 direction, float deltaTime)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        float lerpFactor = 1f - Mathf.Exp(-settings.RootFollowSmoothness * deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpFactor);
    }

    private bool TryGetTargetPosition(out Vector3 targetPosition)
    {
        if (testTargetPoint != null)
        {
            targetPosition = testTargetPoint.position;
            targetPosition.y = transform.position.y;
            return true;
        }

        if (hasStaticTargetPoint)
        {
            targetPosition = staticTargetPoint;
            targetPosition.y = transform.position.y;
            return true;
        }

        targetPosition = transform.position;
        return false;
    }

    private void RaiseMovementTargetReached()
    {
        if (movementTargetReachedRaised)
            return;

        movementTargetReachedRaised = true;
        MovementTargetReached?.Invoke();

        if (!stopOnTargetReached)
            movementTargetReachedRaised = false;
    }

    private void PruneSoldiers()
    {
        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            if (soldiers[i] != null)
                continue;

            soldiers.RemoveAt(i);
        }
    }
}
