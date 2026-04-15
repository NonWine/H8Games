using System;
using UnityEngine;
using Zenject;

public class SquadMoveProvider : ITickable, IMoveProvider
{
    private readonly Transform rootTransform;
    private readonly SquadFollowSettings settings;
    private readonly float targetReachThreshold;

    private Vector3 targetPoint;
    private Action onReached;
    private bool reachedPath;


    public SquadMoveProvider(Transform rootTransform, SquadFollowSettings settings, float targetReachThreshold) : base()
    {
        this.rootTransform = rootTransform;
        this.settings = settings;
        this.targetReachThreshold = targetReachThreshold;
        reachedPath = true;
    }

    public void SetTarget(Vector3 worldPosition, Action onReached = null)
    {
        targetPoint = worldPosition;
        this.onReached = onReached;
        reachedPath = false;
    }

    public void Tick()
    {
        if(reachedPath) return;
        
        Vector3 currentPosition = rootTransform.position;
        targetPoint.y = currentPosition.y;

        Vector3 toTarget = targetPoint - currentPosition;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= targetReachThreshold * targetReachThreshold)
        {
            reachedPath = true;
            onReached?.Invoke();
            return;
        }

        Vector3 direction = toTarget.normalized;

        rootTransform.position = Vector3.MoveTowards(
            currentPosition,
            targetPoint,
            settings.RootMoveSpeed * Time.deltaTime);

        RotateTowards(direction, Time.deltaTime);
    }

    public void Stop()
    {
        reachedPath = true;
        onReached = null;
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
}
