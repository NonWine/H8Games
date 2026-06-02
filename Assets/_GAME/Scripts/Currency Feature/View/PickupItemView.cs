using System;
using DG.Tweening;
using UnityEngine;

public sealed class PickupItemView : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform  visualRoot;
    [SerializeField] private Rigidbody  rb;
    [SerializeField] private Collider[] colliders;

    [Header("Scale Juice")]
    [SerializeField, Min(0f)] private float spawnScaleDuration   = 0.2f;
    [SerializeField, Min(0f)] private float despawnScaleDuration = 0.15f;

    public bool IsRented { get; private set; }

    public Transform             Transform { get; private set; }
    public PickupPhysicsHandler   Physics   { get; private set; }
    public PickupAnimationHandler Animation { get; private set; }

    private Action activePose;
    private Tween  scaleTween;

    private void Awake()
    {
        Transform = transform;
        Physics   = new PickupPhysicsHandler(transform, rb, colliders);
        Animation = new PickupAnimationHandler(transform, rb, visualRoot);
    }

    private void Reset()
    {
        visualRoot = transform;
        rb         = GetComponent<Rigidbody>();
        colliders  = GetComponentsInChildren<Collider>(true);
    }

    private void LateUpdate()
    {
        activePose?.Invoke();
    }

    public void SetActivePose(Action pose)
    {
        activePose = pose;
    }

    public void Rent()
    {
        IsRented   = true;
        activePose = null;

        PlaySpawnScale();
    }

    public void Cleanup()
    {
        IsRented   = false;
        activePose = null;

        scaleTween?.Kill();
        scaleTween = null;

        Animation.ResetAnimationState();
        Animation.ResetVisualState();
        Physics.RestoreDefaults();
        Transform.SetParent(null, true);
    }

    public void PlayDespawnScale(Action onComplete)
    {
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
