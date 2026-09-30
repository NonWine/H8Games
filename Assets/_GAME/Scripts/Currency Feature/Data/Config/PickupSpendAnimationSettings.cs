using System;
using UnityEngine;

[Serializable]
public class PickupSpendAnimationSettings
{
    public Vector2 ArcMultiplier = new Vector2(0.85f, 1.15f);
    [Min(0f)] public float LateralOffset = 0.25f;
    public Vector2 SpinMultiplier = new Vector2(0.75f, 1.25f);
    [Range(0f, 180f)] public float InitialTilt = 45f;
    public AnimationCurve Scale = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.18f, 1.1f),
        new Keyframe(0.32f, 1f), new Keyframe(0.65f, 1f),
        new Keyframe(1f, 0f));

    public PickupSpendAnimationSettings Clone()
    {
        return JsonUtility.FromJson<PickupSpendAnimationSettings>(JsonUtility.ToJson(this));
    }
}
