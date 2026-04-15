using System;
using UnityEngine;

public sealed class SquadRootMover
{
    public Action MovementTargetReached;

    private readonly Transform rootTransform;
    private readonly SquadFollowSettings settings;
    private readonly float targetReachThreshold;

    private Vector3 staticTargetPoint;
    private bool movementTargetReachedRaised;

    public Vector3 HomePosition { get; }
    public Quaternion HomeRotation { get; }

    public bool IsMoving { get; private set; }

    public SquadRootMover(
        Transform rootTransform,
        SquadFollowSettings settings,
        float targetReachThreshold)
    {
        this.rootTransform = rootTransform;
        this.settings = settings;
        this.targetReachThreshold = targetReachThreshold;

        HomePosition = rootTransform.position;
        HomeRotation = rootTransform.rotation;
    }

    public void Tick(float deltaTime)
    {
        if (!IsMoving)
            return;

        staticTargetPoint.y = rootTransform.position.y;

        Vector3 toTarget = staticTargetPoint - rootTransform.position;
        toTarget.y = 0f;
        Vector3 direction = toTarget.normalized;
        rootTransform.position = Vector3.MoveTowards(rootTransform.position, staticTargetPoint, settings.RootMoveSpeed * deltaTime);
        RotateTowards(direction, deltaTime);
        
        if (toTarget.sqrMagnitude <= targetReachThreshold * targetReachThreshold)
        {
            RaiseMovementTargetReached();
        }
    }

    public void MoveTo(Vector3 worldPosition, Action movementTargetReached = null)
    {
        MovementTargetReached = movementTargetReached;
        staticTargetPoint = worldPosition;
        movementTargetReachedRaised = false;
        IsMoving = true;
    }

    public void ReturnHome(Action  movementTargetReached = null)
    {
        MoveTo(HomePosition,  movementTargetReached);
    }

    public void EnterHomeIdle()
    {
        rootTransform.position = HomePosition;
        rootTransform.rotation = HomeRotation;
        movementTargetReachedRaised = false;
        IsMoving = false;
    }

    public void Stop()
    {
        IsMoving = false;
    }

    private void RotateTowards(Vector3 direction, float deltaTime)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        float lerpFactor = 1f - Mathf.Exp(-settings.RootFollowSmoothness * deltaTime);
        rootTransform.rotation = Quaternion.Slerp(rootTransform.rotation, targetRotation, lerpFactor);
    }

    private void RaiseMovementTargetReached()
    {
        if (movementTargetReachedRaised)
            return;

        movementTargetReachedRaised = true;
        MovementTargetReached?.Invoke();
        MovementTargetReached = null;
        Stop();
    }
}