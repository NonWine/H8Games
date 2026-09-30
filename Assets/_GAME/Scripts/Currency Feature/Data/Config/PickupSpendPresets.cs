using UnityEngine;

public static class PickupSpendPresets
{
    public static PickupSpendAnimationSettings Create(PickupSpendPreset preset)
    {
        switch (preset)
        {
            case PickupSpendPreset.SnappyDart: return SnappyDart();
            case PickupSpendPreset.ElasticLaunch: return ElasticLaunch();
            case PickupSpendPreset.WeightyToss: return WeightyToss();
            case PickupSpendPreset.SpinFrenzy: return SpinFrenzy();
            case PickupSpendPreset.GracefulArc: return GracefulArc();
            case PickupSpendPreset.PopFizzle: return PopFizzle();
            case PickupSpendPreset.TurboZip: return TurboZip();
            default: return Balanced();
        }
    }

    // Current tuned default.
    private static PickupSpendAnimationSettings Balanced()
    {
        return new PickupSpendAnimationSettings();
    }

    // Fast, low arc, barely any spin variance - quick in, quick gone.
    private static PickupSpendAnimationSettings SnappyDart()
    {
        return new PickupSpendAnimationSettings
        {
            ArcMultiplier = new Vector2(0.5f, 0.7f), LateralOffset = 0.12f, SpinMultiplier = new Vector2(0.4f, 0.6f), InitialTilt = 20f,
            Scale = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.12f, 1.05f), new Keyframe(0.25f, 1f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0f)),
        };
    }

    // Big overshoot arc, strong spin, elastic pop on launch.
    private static PickupSpendAnimationSettings ElasticLaunch()
    {
        return new PickupSpendAnimationSettings
        {
            ArcMultiplier = new Vector2(1.3f, 1.7f), LateralOffset = 0.35f, SpinMultiplier = new Vector2(1.2f, 1.6f), InitialTilt = 60f,
            Scale = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.15f, 1.35f), new Keyframe(0.28f, 0.85f),
                new Keyframe(0.42f, 1.1f), new Keyframe(0.6f, 1f), new Keyframe(1f, 0f)),
        };
    }

    // Slower, heavier arc with a wide lateral wobble.
    private static PickupSpendAnimationSettings WeightyToss()
    {
        return new PickupSpendAnimationSettings
        {
            ArcMultiplier = new Vector2(1.5f, 2.1f), LateralOffset = 0.45f, SpinMultiplier = new Vector2(0.5f, 0.8f), InitialTilt = 35f,
            Scale = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.25f, 1.08f), new Keyframe(0.4f, 1f), new Keyframe(0.75f, 1f), new Keyframe(1f, 0f)),
        };
    }

    // Chaotic, high spin variance - playful and unpredictable.
    private static PickupSpendAnimationSettings SpinFrenzy()
    {
        return new PickupSpendAnimationSettings
        {
            ArcMultiplier = new Vector2(0.9f, 1.3f), LateralOffset = 0.3f, SpinMultiplier = new Vector2(2.2f, 3f), InitialTilt = 90f,
            Scale = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.15f, 1.1f), new Keyframe(0.3f, 1f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0f)),
        };
    }

    // Smooth, moderate, minimal spin - the elegant choice.
    private static PickupSpendAnimationSettings GracefulArc()
    {
        return new PickupSpendAnimationSettings
        {
            ArcMultiplier = new Vector2(1f, 1.2f), LateralOffset = 0.1f, SpinMultiplier = new Vector2(0.3f, 0.45f), InitialTilt = 15f,
            Scale = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.22f, 1.04f), new Keyframe(0.4f, 1f), new Keyframe(0.75f, 1f), new Keyframe(1f, 0f)),
        };
    }

    // Big early scale spike that fizzles out before it arrives.
    private static PickupSpendAnimationSettings PopFizzle()
    {
        return new PickupSpendAnimationSettings
        {
            ArcMultiplier = new Vector2(0.8f, 1f), LateralOffset = 0.2f, SpinMultiplier = new Vector2(0.8f, 1.1f), InitialTilt = 40f,
            Scale = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.1f, 1.4f), new Keyframe(0.25f, 0.9f),
                new Keyframe(0.45f, 1f), new Keyframe(0.7f, 0.4f), new Keyframe(1f, 0f)),
        };
    }

    // Fastest, tightest arc, almost no lateral drift - a straight rocket to the target.
    private static PickupSpendAnimationSettings TurboZip()
    {
        return new PickupSpendAnimationSettings
        {
            ArcMultiplier = new Vector2(0.35f, 0.5f), LateralOffset = 0.05f, SpinMultiplier = new Vector2(0.9f, 1.1f), InitialTilt = 10f,
            Scale = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.1f, 1.05f), new Keyframe(0.2f, 1f), new Keyframe(0.75f, 1f), new Keyframe(1f, 0f)),
        };
    }
}
