using UnityEngine;

public static class PickupIdlePresets
{
    public static PickupIdleAnimationSettings Create(PickupIdlePreset preset)
    {
        switch (preset)
        {
            case PickupIdlePreset.CuriousPeek: return CuriousPeek();
            case PickupIdlePreset.LazyDrift: return LazyDrift();
            case PickupIdlePreset.PerkyBounce: return PerkyBounce();
            case PickupIdlePreset.ElasticWobble: return ElasticWobble();
            case PickupIdlePreset.SharpTwitch: return SharpTwitch();
            case PickupIdlePreset.FloatyGlow: return FloatyGlow();
            case PickupIdlePreset.RoyalShimmer: return RoyalShimmer();
            default: return Balanced();
        }
    }

    // Current tuned default - a clean single hop with a tiny settle bounce.
    private static PickupIdleAnimationSettings Balanced()
    {
        return new PickupIdleAnimationSettings();
    }

    // Frequent, short, alert little peeks - like it noticed you.
    private static PickupIdleAnimationSettings CuriousPeek()
    {
        return new PickupIdleAnimationSettings
        {
            Interval = new Vector2(1.5f, 2.8f), Duration = 0.5f, HopHeight = 0.18f, TiltDegrees = 22f, ShineTime = 0.35f,
            Height = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.12f, 0f), new Keyframe(0.3f, 1f),
                new Keyframe(0.55f, 0.15f), new Keyframe(0.7f, 0.6f), new Keyframe(1f, 0f)),
            Tilt = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.12f, -0.08f), new Keyframe(0.3f, 1f),
                new Keyframe(0.55f, 0.1f), new Keyframe(0.7f, 0.5f), new Keyframe(1f, 0f)),
        };
    }

    // Rare, slow, smooth sway - barely bothers to move.
    private static PickupIdleAnimationSettings LazyDrift()
    {
        return new PickupIdleAnimationSettings
        {
            Interval = new Vector2(5f, 8f), Duration = 1.4f, HopHeight = 0.16f, TiltDegrees = 14f, ShineTime = 0.55f,
            Height = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.3f, 0.55f), new Keyframe(0.5f, 1f),
                new Keyframe(0.75f, 0.9f), new Keyframe(1f, 0f)),
            Tilt = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.3f, 0.4f), new Keyframe(0.5f, 1f),
                new Keyframe(0.75f, 0.85f), new Keyframe(1f, 0f)),
        };
    }

    // Energetic overshoot into a double-bounce before it settles.
    private static PickupIdleAnimationSettings PerkyBounce()
    {
        return new PickupIdleAnimationSettings
        {
            Interval = new Vector2(2f, 3.5f), Duration = 0.9f, HopHeight = 0.36f, TiltDegrees = 38f, ShineTime = 0.4f,
            Height = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.12f, 0f), new Keyframe(0.35f, 1.15f), new Keyframe(0.5f, 0.7f),
                new Keyframe(0.65f, 1f), new Keyframe(0.85f, 0f), new Keyframe(0.93f, 0.08f), new Keyframe(1f, 0f)),
            Tilt = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.12f, -0.15f), new Keyframe(0.35f, 1.1f), new Keyframe(0.5f, 0.6f),
                new Keyframe(0.65f, 1f), new Keyframe(0.85f, 0f), new Keyframe(0.93f, -0.08f), new Keyframe(1f, 0f)),
        };
    }

    // Springy oscillation that rings before it settles.
    private static PickupIdleAnimationSettings ElasticWobble()
    {
        return new PickupIdleAnimationSettings
        {
            Interval = new Vector2(2.5f, 4f), Duration = 1.1f, HopHeight = 0.3f, TiltDegrees = 30f, ShineTime = 0.5f,
            Height = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.2f, 1.2f), new Keyframe(0.35f, 0.8f), new Keyframe(0.5f, 1.05f),
                new Keyframe(0.65f, 0.92f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0f)),
            Tilt = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.2f, 1.15f), new Keyframe(0.35f, 0.75f), new Keyframe(0.5f, 1.05f),
                new Keyframe(0.65f, 0.9f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0f)),
        };
    }

    // Fast, sharp, almost mechanical - snaps up, holds, snaps down.
    private static PickupIdleAnimationSettings SharpTwitch()
    {
        return new PickupIdleAnimationSettings
        {
            Interval = new Vector2(1.2f, 2.2f), Duration = 0.32f, HopHeight = 0.14f, TiltDegrees = 40f, ShineTime = 0.25f,
            Height = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.08f, 1f), new Keyframe(0.5f, 1f), new Keyframe(0.6f, 0f), new Keyframe(1f, 0f)),
            Tilt = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.08f, 1f), new Keyframe(0.5f, 1f), new Keyframe(0.6f, 0f), new Keyframe(1f, 0f)),
        };
    }

    // Slow dreamy rise with a long lingering shine.
    private static PickupIdleAnimationSettings FloatyGlow()
    {
        return new PickupIdleAnimationSettings
        {
            Interval = new Vector2(4.5f, 7f), Duration = 1.8f, HopHeight = 0.22f, TiltDegrees = 18f, ShineTime = 0.6f,
            Height = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.4f, 0.5f), new Keyframe(0.6f, 1f), new Keyframe(0.85f, 0.85f), new Keyframe(1f, 0f)),
            Tilt = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.4f, 0.4f), new Keyframe(0.6f, 1f), new Keyframe(0.85f, 0.8f), new Keyframe(1f, 0f)),
        };
    }

    // Tall, confident hop with a strong lean and an early, proud shine.
    private static PickupIdleAnimationSettings RoyalShimmer()
    {
        return new PickupIdleAnimationSettings
        {
            Interval = new Vector2(3f, 4.5f), Duration = 0.85f, HopHeight = 0.42f, TiltDegrees = 46f, ShineTime = 0.3f,
            Height = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.15f, 0f), new Keyframe(0.4f, 1f),
                new Keyframe(0.75f, 0f), new Keyframe(0.88f, 0.06f), new Keyframe(1f, 0f)),
            Tilt = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.15f, -0.14f), new Keyframe(0.4f, 1f),
                new Keyframe(0.75f, 0f), new Keyframe(0.88f, -0.07f), new Keyframe(1f, 0f)),
        };
    }
}
