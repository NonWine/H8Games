using System;
using UnityEngine;

public sealed class PickupItemView : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform  visualRoot;
    [SerializeField] private Rigidbody  rb;
    [SerializeField] private Collider[] colliders;

    public bool IsRented { get; private set; }

    public Transform             Transform { get; private set; }
    public PickupPhysicsHandler   Physics   { get; private set; }
    public PickupAnimationHandler Animation { get; private set; }

    private Action activePose;

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
    }

    public void Cleanup()
    {
        IsRented   = false;
        activePose = null;

        Animation.ResetAnimationState();
        Animation.ResetVisualState();
        Physics.RestoreDefaults();
        Transform.SetParent(null, true);
    }
}
