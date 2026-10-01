using System;
using DG.Tweening;
using UnityEngine;

public class PickupItemView : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform  visualRoot;
    [SerializeField] private Rigidbody  rb;
    [SerializeField] private Collider[] colliders;

    [Header("FX")]
    [SerializeField] private ParticleSystem impactFx;
    [SerializeField] private ParticleSystem idleShineFx;

    [Header("Scale Juice")]
    [SerializeField, Min(0f)] private float spawnScaleDuration   = 0.2f;
    [SerializeField, Min(0f)] private float despawnScaleDuration = 0.15f;

    public bool IsRented { get; private set; }

    public Transform              Transform { get; private set; }
    public PickupPhysicsHandler   Physics   { get; private set; }
    public PickupAnimationHandler Animation { get; private set; }
    public PickupImpactFxHandler  Impact    { get; private set; }
    public PickupIdleHandler Idle { get; private set; }

    private PickupStackImpactHandler stackImpact;

    private Action activePose;
    private Tween  scaleTween;

    private void Awake()
    {
        Transform = transform;
        Physics   = new PickupPhysicsHandler(transform, rb, colliders);
        Animation = new PickupAnimationHandler(transform, rb, visualRoot);
        Impact    = new PickupImpactFxHandler(impactFx);
        if (visualRoot == transform || !visualRoot.IsChildOf(transform))
            throw new InvalidOperationException("Pickup visualRoot must be a separate visual child.");
        Idle = new PickupIdleHandler(visualRoot, Physics, idleShineFx);
        stackImpact = new PickupStackImpactHandler(visualRoot);
    }

    private void Reset()
    {
        visualRoot = transform;
        rb         = GetComponent<Rigidbody>();
        colliders  = GetComponentsInChildren<Collider>(true);
    }

    private void FixedUpdate()
    {
        Physics.ApplyExtraGravity();
    }

    private void LateUpdate()
    {
        activePose?.Invoke();
        Idle.Tick(Time.deltaTime);
        stackImpact.Tick(Time.deltaTime);
    }

    private void OnDisable()
    {
        Idle?.Stop();
        stackImpact?.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Physics.TrackSupport(collision);
        Impact.HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        Physics.TrackSupport(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        Physics.RemoveSupport(collision);
    }

    public void PlayStackImpact(PickupStackImpactSettings settings, float strength, float dipStrength, float delay)
    {
        scaleTween?.Kill();
        scaleTween = null;

        stackImpact.Play(settings, strength, dipStrength, delay);

        if (settings.PlayLandingFx && delay <= 0f)
            Impact.PlayAtSelf();
    }

    public void PlayJuicyLandingSquash(Vector3 punchScale, float duration)
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(punchScale, duration, 4, 0.5f)
                 .SetLink(gameObject);
    }

    public void SetCarrySway(Vector3 worldOffset, float tiltDegrees, Vector3 worldTiltAxis, float smoothTime)
    {
        var parent = visualRoot.parent;

        if (parent == null)
            return;

        var localOffset = parent.InverseTransformVector(worldOffset);
        var localEuler  = Vector3.zero;

        if (tiltDegrees > 0.01f && worldTiltAxis.sqrMagnitude > 0.0001f)
            localEuler = ToSignedEuler(Quaternion.AngleAxis(tiltDegrees, parent.InverseTransformDirection(worldTiltAxis)).eulerAngles);

        Animation.SetSecondaryMotion(localOffset, localEuler, smoothTime);
    }

    private static Vector3 ToSignedEuler(Vector3 euler)
    {
        return new Vector3(SignedAngle(euler.x), SignedAngle(euler.y), SignedAngle(euler.z));
    }

    private static float SignedAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    public void StopStackImpact()
    {
        stackImpact.Stop();
    }

    public void SetActivePose(Action pose)
    {
        activePose = pose;
    }

    public void Rent()
    {
        Idle.Stop();
        stackImpact.Stop();
        IsRented   = true;
        activePose = null;

        PlaySpawnScale();
    }

    public void Cleanup()
    {
        IsRented   = false;
        activePose = null;

        Impact.Disable();
        stackImpact.Stop();

        scaleTween?.Kill();
        scaleTween = null;

        // Scene teardown destroys pooled views before PickupService disposes and
        // drains the registry, so the pool can hand back an already-destroyed view.
        if (this == null)
            return;

        transform.DOKill();
        transform.localScale = Vector3.one;

        Idle.Stop();
        Animation.ResetAnimationState();
        Animation.ResetVisualState();
        Physics.RestoreDefaults();
        Transform.SetParent(null, true);
    }

    public void PlayDespawnScale(Action onComplete)
    {
        Idle.Stop();
        stackImpact.Stop();
        scaleTween?.Kill();
        scaleTween = visualRoot
            .DOScale(Vector3.zero, despawnScaleDuration)
            .SetEase(Ease.InBack)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                scaleTween = null;
                onComplete?.Invoke();
            });
    }

    public void PrepareSpendPresentation()
    {
        scaleTween?.Kill();
        scaleTween = null;
        Animation.ResetVisualState();
    }

    private void PlaySpawnScale()
    {
        scaleTween?.Kill();

        Vector3 baseScale = visualRoot.localScale;

        visualRoot.localScale = Vector3.zero;
        scaleTween = visualRoot
            .DOScale(baseScale, spawnScaleDuration)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);
    }
}
