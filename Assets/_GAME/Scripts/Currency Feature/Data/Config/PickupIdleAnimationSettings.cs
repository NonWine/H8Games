using System;
using UnityEngine;

[Serializable]
public class PickupIdleAnimationSettings
{
    public Vector2 Interval = new Vector2(3f, 5f);
    [Min(0.01f)] public float Duration = 0.8f;
    [Min(0f)] public float HopHeight = 0.28f;
    [Range(0f, 90f)] public float TiltDegrees = 32f;
    [Range(0f, 1f)] public float ShineTime = 0.45f;
    public AnimationCurve Height = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.15f, 0f),
        new Keyframe(0.45f, 1f), new Keyframe(0.8f, 0f),
        new Keyframe(0.9f, 0.04f), new Keyframe(1f, 0f));
    public AnimationCurve Tilt = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.15f, -0.12f),
        new Keyframe(0.45f, 1f), new Keyframe(0.8f, 0f),
        new Keyframe(0.9f, -0.06f), new Keyframe(1f, 0f));

    public PickupIdleAnimationSettings Clone()
    {
        return JsonUtility.FromJson<PickupIdleAnimationSettings>(JsonUtility.ToJson(this));
    }
}
