using System;
using UnityEngine;

[Serializable]
public class PickupSpendSettings
{
    [SerializeField] private Vector2 arcMultiplier = new Vector2(0.85f, 1.15f);
    [SerializeField, Min(0f)] private float lateralOffset = 0.25f;
    [SerializeField] private Vector2 spinMultiplier = new Vector2(0.75f, 1.25f);
    [SerializeField, Range(0f, 180f)] private float initialTilt = 45f;
    [SerializeField] private AnimationCurve scale = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.18f, 1.1f),
        new Keyframe(0.32f, 1f), new Keyframe(0.65f, 1f),
        new Keyframe(1f, 0f));

    public Vector2 ArcMultiplier => arcMultiplier;
    public float LateralOffset => lateralOffset;
    public Vector2 SpinMultiplier => spinMultiplier;
    public float InitialTilt => initialTilt;
    public AnimationCurve Scale => scale;
}
