using System;
using UnityEngine;

public class PickupAnimationHandler
{
    private readonly Transform transform;
    private readonly Rigidbody rb;
    private readonly Transform visualRoot;
    private readonly Vector3   initVisualLocalPos;
    private readonly Quaternion initVisualLocalRot;
    private readonly Vector3   initVisualLocalScale;

    private Transform  collectAnchor;
    private Vector3    collectStartWorldPos;
    private Quaternion collectStartWorldRot;
    private Vector3    collectTargetLocalPos;
    private Quaternion collectTargetLocalRot;
    private float      collectElapsed;
    private Action     collectCompleted;

    private Transform  carryAnchor;
    private Vector3    currentCarryLocalPos;
    private Quaternion currentCarryLocalRot;
    private Vector3    targetCarryLocalPos;
    private Quaternion targetCarryLocalRot;

    private bool       isMovingToSlot;
    private float      moveElapsed;
    private Vector3    moveStartLocalPos;
    private Quaternion moveStartLocalRot;

    private Vector3 secondaryCurrentPosOffset;
    private Vector3 secondaryTargetPosOffset;
    private Vector3 secondaryPosVelocity;
    private Vector3 secondaryCurrentEuler;
    private Vector3 secondaryTargetEuler;
    private Vector3 secondaryEulerVelocity;
    private float   secondarySmoothTime;

    private Transform  spendTarget;
    private Vector3    spendStartPos;
    private Quaternion spendStartRot;
    private float      spendElapsed;
    private Action     spendCompleted;

    public Action     CollectCompleted      => collectCompleted;
    public Transform  CollectAnchor         => collectAnchor;
    public Vector3    CollectTargetLocalPos  => collectTargetLocalPos;
    public Quaternion CollectTargetLocalRot  => collectTargetLocalRot;
    public Action     SpendCompleted         => spendCompleted;

    public PickupAnimationHandler(Transform transform, Rigidbody rb, Transform visualRoot)
    {
        this.transform  = transform;
        this.rb         = rb;
        this.visualRoot = visualRoot;

        initVisualLocalPos   = visualRoot.localPosition;
        initVisualLocalRot   = visualRoot.localRotation;
        initVisualLocalScale = visualRoot.localScale;
    }

    public void BeginCollect(Transform anchor, Vector3 localTargetPos, Quaternion localTargetRot, Action onCompleted)
    {
        collectAnchor         = anchor;
        collectTargetLocalPos = localTargetPos;
        collectTargetLocalRot = localTargetRot;
        collectStartWorldPos  = transform.position;
        collectStartWorldRot  = transform.rotation;
        collectElapsed        = 0f;
        collectCompleted      = onCompleted;
    }

    public bool TickCollect(float deltaTime, float duration)
    {
        collectElapsed += deltaTime;
        return collectElapsed >= Mathf.Max(0.0001f, duration);
    }

    public void ApplyCollectPose(float duration, float arcHeight, AnimationCurve curve)
    {
        var t              = Mathf.Clamp01(collectElapsed / Mathf.Max(0.0001f, duration));
        var eased          = curve.Evaluate(t);
        var targetWorldPos = collectAnchor.TransformPoint(collectTargetLocalPos);
        var targetWorldRot = collectAnchor.rotation * collectTargetLocalRot;
        var pos            = Vector3.Lerp(collectStartWorldPos, targetWorldPos, eased);

        pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.SetPositionAndRotation(pos, Quaternion.Slerp(collectStartWorldRot, targetWorldRot, eased));
        SyncKinematicRigidbody();
    }

    public void BeginCarry(Transform anchor, Vector3 localPos, Quaternion localRot)
    {
        carryAnchor          = anchor;
        currentCarryLocalPos = localPos;
        currentCarryLocalRot = localRot;
        targetCarryLocalPos  = localPos;
        targetCarryLocalRot  = localRot;
        isMovingToSlot       = false;
        moveElapsed          = 0f;
    }

    public void MoveToCarrySlot(Transform anchor, Vector3 localPos, Quaternion localRot)
    {
        carryAnchor = anchor;

        if (targetCarryLocalPos == localPos && targetCarryLocalRot == localRot)
            return;

        moveStartLocalPos   = currentCarryLocalPos;
        moveStartLocalRot   = currentCarryLocalRot;
        targetCarryLocalPos = localPos;
        targetCarryLocalRot = localRot;
        moveElapsed         = 0f;
        isMovingToSlot      = true;
    }

    public void SetSecondaryMotion(Vector3 posOffset, Vector3 eulerOffset, float smoothTime)
    {
        secondaryTargetPosOffset = posOffset;
        secondaryTargetEuler     = eulerOffset;
        secondarySmoothTime      = smoothTime;
    }

    public void TickCarry(float deltaTime, float moveToSlotDuration)
    {
        TickCarrySlotMove(deltaTime, moveToSlotDuration);
        TickSecondaryMotion(deltaTime);
    }

    public void ApplyCarryPose()
    {
        var worldPos = carryAnchor.TransformPoint(currentCarryLocalPos);
        var worldRot = carryAnchor.rotation * currentCarryLocalRot;

        transform.SetPositionAndRotation(worldPos, worldRot);
        ApplySecondaryVisualOffset();
        SyncKinematicRigidbody();
    }

    public void BeginSpend(Transform target, Action onCompleted)
    {
        spendTarget    = target;
        spendStartPos  = transform.position;
        spendStartRot  = transform.rotation;
        spendElapsed   = 0f;
        spendCompleted = onCompleted;
    }

    public bool TickSpend(float deltaTime, float duration)
    {
        spendElapsed += deltaTime;
        return spendElapsed >= Mathf.Max(0.0001f, duration);
    }

    public void ApplySpendPose(float duration, float jumpPower, float spinSpeed, AnimationCurve curve)
    {
        var t      = Mathf.Clamp01(spendElapsed / Mathf.Max(0.0001f, duration));
        var eased  = curve.Evaluate(t);
        var endPos = spendTarget.position;
        var pos    = Vector3.Lerp(spendStartPos, endPos, eased);

        pos.y += 4f * jumpPower * t * (1f - t);

        transform.position = pos;

        if (spinSpeed != 0f)
            transform.rotation = Quaternion.AngleAxis(spinSpeed * spendElapsed, Vector3.up) * spendStartRot;

        SyncKinematicRigidbody();
    }

    public void ResetAnimationState()
    {
        collectAnchor         = null;
        collectStartWorldPos  = Vector3.zero;
        collectStartWorldRot  = Quaternion.identity;
        collectTargetLocalPos = Vector3.zero;
        collectTargetLocalRot = Quaternion.identity;
        collectElapsed        = 0f;
        collectCompleted      = null;

        carryAnchor          = null;
        currentCarryLocalPos = Vector3.zero;
        currentCarryLocalRot = Quaternion.identity;
        targetCarryLocalPos  = Vector3.zero;
        targetCarryLocalRot  = Quaternion.identity;
        moveStartLocalPos    = Vector3.zero;
        moveStartLocalRot    = Quaternion.identity;
        moveElapsed          = 0f;
        isMovingToSlot       = false;

        secondaryCurrentPosOffset = Vector3.zero;
        secondaryTargetPosOffset  = Vector3.zero;
        secondaryPosVelocity      = Vector3.zero;
        secondaryCurrentEuler     = Vector3.zero;
        secondaryTargetEuler      = Vector3.zero;
        secondaryEulerVelocity    = Vector3.zero;
        secondarySmoothTime       = 0f;

        spendTarget    = null;
        spendStartPos  = Vector3.zero;
        spendStartRot  = Quaternion.identity;
        spendElapsed   = 0f;
        spendCompleted = null;
    }

    public void ResetVisualState()
    {
        visualRoot.localPosition = initVisualLocalPos;
        visualRoot.localRotation = initVisualLocalRot;
        visualRoot.localScale    = initVisualLocalScale;
    }

    private void TickCarrySlotMove(float deltaTime, float duration)
    {
        if (!isMovingToSlot)
            return;

        moveElapsed += deltaTime;

        var progress = Mathf.Clamp01(moveElapsed / Mathf.Max(0.0001f, duration));

        currentCarryLocalPos = Vector3.Lerp(moveStartLocalPos, targetCarryLocalPos, progress);
        currentCarryLocalRot = Quaternion.Slerp(moveStartLocalRot, targetCarryLocalRot, progress);

        if (progress >= 1f)
        {
            currentCarryLocalPos = targetCarryLocalPos;
            currentCarryLocalRot = targetCarryLocalRot;
            isMovingToSlot       = false;
        }
    }

    private void TickSecondaryMotion(float deltaTime)
    {
        var safeSmoothTime = Mathf.Max(0f, secondarySmoothTime);

        if (safeSmoothTime <= 0.0001f)
        {
            secondaryCurrentPosOffset = secondaryTargetPosOffset;
            secondaryCurrentEuler     = secondaryTargetEuler;
            secondaryPosVelocity      = Vector3.zero;
            secondaryEulerVelocity    = Vector3.zero;
            return;
        }

        secondaryCurrentPosOffset = Vector3.SmoothDamp(
            secondaryCurrentPosOffset, secondaryTargetPosOffset,
            ref secondaryPosVelocity, safeSmoothTime, Mathf.Infinity, deltaTime);

        secondaryCurrentEuler = Vector3.SmoothDamp(
            secondaryCurrentEuler, secondaryTargetEuler,
            ref secondaryEulerVelocity, safeSmoothTime, Mathf.Infinity, deltaTime);
    }

    private void ApplySecondaryVisualOffset()
    {
        if (visualRoot == transform)
            return;

        visualRoot.localPosition = initVisualLocalPos + secondaryCurrentPosOffset;
        visualRoot.localRotation = initVisualLocalRot * Quaternion.Euler(secondaryCurrentEuler);
    }

    private void SyncKinematicRigidbody()
    {
        rb.position = transform.position;
        rb.rotation = transform.rotation;
    }
}
