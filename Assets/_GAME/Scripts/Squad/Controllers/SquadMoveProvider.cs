using System;
using UnityEngine;
using Zenject;

public class SquadMoveProvider : ITickable, IMoveProvider , ISquadMovementStateReader
{
    // Keeps the braking curve from going asymptotic so the root still lands
    // exactly on targets that use a zero reach threshold (the home position).
    private const float MinApproachSpeed = 0.75f;

    private readonly SquadRootView rootTransform;
    private readonly SquadFollowSettings settings;

    private Vector3 targetPoint;
    private Action onReached;
    private float currentSpeed;
    private bool reachedPath;


    public SquadMoveProvider(SquadRootView rootTransform, SquadFollowSettings settings) 
    {
        this.rootTransform = rootTransform;
        this.settings = settings;
        reachedPath = true;
    }

    public void SetTarget(Vector3 worldPosition, Action onReached = null)
    {
        targetPoint = worldPosition;
        this.onReached = onReached;
        reachedPath = false;
        currentSpeed = 0f;
    }

    public void Tick()
    {
        if(reachedPath) return;
        IsMoving = true;
        Vector3 currentPosition = rootTransform.transform.position;
        targetPoint.y = currentPosition.y;

        Vector3 toTarget = targetPoint - currentPosition;
        toTarget.y = 0f;
        var reachThreshold = rootTransform.TargetReachThreshold * rootTransform.TargetReachThreshold;
        if (targetPoint == rootTransform.HomePosition)
        {
            reachThreshold = 0f;
        }
        
        if (toTarget.sqrMagnitude <= reachThreshold)
        {
            reachedPath = true;
            IsMoving = false;
            onReached?.Invoke();
            return;
        }

        Vector3 direction = toTarget.normalized;

        rootTransform.transform.position = Vector3.MoveTowards(
            currentPosition,
            targetPoint,
            GetApproachSpeed(toTarget.magnitude, Time.deltaTime) * Time.deltaTime);

        RotateTowards(direction, Time.deltaTime, reachThreshold.Equals(0f) && toTarget.sqrMagnitude <= 3f);
    }

    public void Stop()
    {
        reachedPath = true;
        onReached = null;
        currentSpeed = 0f;
    }

    // Ramps in and brakes on approach instead of translating at a flat speed,
    // which is what made the whole formation read like it was on a conveyor belt.
    private float GetApproachSpeed(float distanceToTarget, float deltaTime)
    {
        float acceleration = settings.RootAcceleration;
        float brakingSpeed = Mathf.Sqrt(2f * acceleration * distanceToTarget);
        float desiredSpeed = Mathf.Max(MinApproachSpeed, Mathf.Min(settings.RootMoveSpeed, brakingSpeed));

        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * deltaTime);

        return currentSpeed;
    }

    private void RotateTowards(Vector3 direction, float deltaTime, bool useIdentity = false)
    {
        Quaternion targetRotation;

        if (useIdentity)
        {
            targetRotation = Quaternion.identity;
        }
        else
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        float lerpFactor = 1f - Mathf.Exp(-settings.RootFollowSmoothness * deltaTime);
        rootTransform.transform.rotation = Quaternion.Slerp(
            rootTransform.transform.rotation,
            targetRotation,
            lerpFactor);
    }

    public bool IsMoving { get; private set; }
}
