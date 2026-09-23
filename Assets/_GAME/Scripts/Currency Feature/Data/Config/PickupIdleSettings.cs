using System;
using UnityEngine;

[Serializable]
public class PickupIdleSettings
{
    [SerializeField] private bool enabled = true;
    [SerializeField] private Vector2 interval = new Vector2(3f, 5f);
    [SerializeField, Min(0.01f)] private float duration = 0.8f;
    [SerializeField, Min(0f)] private float settleDuration = 0.25f;
    [SerializeField, Min(0f)] private float maxLinearSpeed = 0.08f;
    [SerializeField, Min(0f)] private float maxAngularSpeed = 0.3f;
    [SerializeField, Range(0f, 1f)] private float minSupportNormal = 0.5f;
    [SerializeField] private LayerMask supportMask = 1;
    [SerializeField, Min(0f)] private float hopHeight = 0.28f;
    [SerializeField, Range(0f, 90f)] private float tiltDegrees = 32f;
    [SerializeField] private Vector3 faceDirection = new Vector3(0f, 0.8f, -0.6f);
    [SerializeField, Range(0f, 1f)] private float shineTime = 0.45f;
    [SerializeField] private AnimationCurve height = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.15f, 0f),
        new Keyframe(0.45f, 1f), new Keyframe(0.8f, 0f),
        new Keyframe(0.9f, 0.04f), new Keyframe(1f, 0f));
    [SerializeField] private AnimationCurve tilt = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.15f, -0.12f),
        new Keyframe(0.45f, 1f), new Keyframe(0.8f, 0f),
        new Keyframe(0.9f, -0.06f), new Keyframe(1f, 0f));

    public bool Enabled => enabled;
    public Vector2 Interval => interval;
    public float Duration => duration;
    public float SettleDuration => settleDuration;
    public float MaxLinearSpeed => maxLinearSpeed;
    public float MaxAngularSpeed => maxAngularSpeed;
    public float MinSupportNormal => minSupportNormal;
    public LayerMask SupportMask => supportMask;
    public float HopHeight => hopHeight;
    public float TiltDegrees => tiltDegrees;
    public Vector3 FaceDirection => faceDirection;
    public float ShineTime => shineTime;
    public AnimationCurve Height => height;
    public AnimationCurve Tilt => tilt;
}
