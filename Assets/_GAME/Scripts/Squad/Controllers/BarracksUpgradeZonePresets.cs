using DG.Tweening;

public static class BarracksUpgradeZonePresets
{
    public static BarracksUpgradeZoneAnimationSettings Create(BarracksUpgradeZonePreset preset)
    {
        switch (preset)
        {
            case BarracksUpgradeZonePreset.SnappyPop: return SnappyPop();
            case BarracksUpgradeZonePreset.ElasticBounce: return ElasticBounce();
            case BarracksUpgradeZonePreset.HeavyImpact: return HeavyImpact();
            case BarracksUpgradeZonePreset.SilkSmooth: return SilkSmooth();
            case BarracksUpgradeZonePreset.TurboRush: return TurboRush();
            case BarracksUpgradeZonePreset.CartoonSquash: return CartoonSquash();
            case BarracksUpgradeZonePreset.FireworkFinish: return FireworkFinish();
            default: return Balanced();
        }
    }

    // Current tuned default - readable, mid-weight, safe baseline.
    private static BarracksUpgradeZoneAnimationSettings Balanced()
    {
        return new BarracksUpgradeZoneAnimationSettings();
    }

    // Fastest read, minimal overshoot - for upgrades that fire often.
    private static BarracksUpgradeZoneAnimationSettings SnappyPop()
    {
        return new BarracksUpgradeZoneAnimationSettings
        {
            ShowDuration = 0.1f, ShowOvershootScale = 1.12f, ShowEase = Ease.OutBack,
            HideDuration = 0.12f, HideEase = Ease.OutQuad,
            FillDuration = 0.12f, FillEase = Ease.OutQuad,
            BaseTossPitch = 1f, TossPitchStep = 0.035f, MaxTossPitch = 1.7f,
            LabelPunch = 0.14f, LabelPunchDuration = 0.08f, LabelPunchVibrato = 1, LabelPunchElasticity = 0.6f,
            ArrivalAccentInterval = 0.06f, ArrivalParticleCount = 1, FinalArrivalMultiplier = 1.4f,
            CompletionShakeStrength = 0.28f, CompletionShakeDuration = 0.18f, CompletionShakeVibrato = 14,
        };
    }

    // Springy OutElastic on panel and fill, big wobbly label punch.
    private static BarracksUpgradeZoneAnimationSettings ElasticBounce()
    {
        return new BarracksUpgradeZoneAnimationSettings
        {
            ShowDuration = 0.32f, ShowOvershootScale = 1.3f, ShowEase = Ease.OutElastic,
            HideDuration = 0.24f, HideEase = Ease.InOutBack,
            FillDuration = 0.35f, FillEase = Ease.OutElastic,
            BaseTossPitch = 1f, TossPitchStep = 0.03f, MaxTossPitch = 1.6f,
            LabelPunch = 0.32f, LabelPunchDuration = 0.22f, LabelPunchVibrato = 2, LabelPunchElasticity = 0.9f,
            ArrivalAccentInterval = 0.1f, ArrivalParticleCount = 1, FinalArrivalMultiplier = 1.7f,
            CompletionShakeStrength = 0.45f, CompletionShakeDuration = 0.4f, CompletionShakeVibrato = 8,
        };
    }

    // Slow and weighty - things slam rather than pop, biggest final hit.
    private static BarracksUpgradeZoneAnimationSettings HeavyImpact()
    {
        return new BarracksUpgradeZoneAnimationSettings
        {
            ShowDuration = 0.26f, ShowOvershootScale = 1.14f, ShowEase = Ease.OutQuint,
            HideDuration = 0.3f, HideEase = Ease.InQuad,
            FillDuration = 0.3f, FillEase = Ease.OutQuart,
            BaseTossPitch = 0.85f, TossPitchStep = 0.02f, MaxTossPitch = 1.3f,
            LabelPunch = 0.22f, LabelPunchDuration = 0.16f, LabelPunchVibrato = 1, LabelPunchElasticity = 0.6f,
            ArrivalAccentInterval = 0.12f, ArrivalParticleCount = 2, FinalArrivalMultiplier = 2f,
            CompletionShakeStrength = 0.65f, CompletionShakeDuration = 0.45f, CompletionShakeVibrato = 6,
        };
    }

    // Gentle sine eases, near-silent shake - reads as premium polish.
    private static BarracksUpgradeZoneAnimationSettings SilkSmooth()
    {
        return new BarracksUpgradeZoneAnimationSettings
        {
            ShowDuration = 0.24f, ShowOvershootScale = 1.08f, ShowEase = Ease.OutSine,
            HideDuration = 0.22f, HideEase = Ease.InOutSine,
            FillDuration = 0.26f, FillEase = Ease.InOutSine,
            BaseTossPitch = 1f, TossPitchStep = 0.02f, MaxTossPitch = 1.35f,
            LabelPunch = 0.1f, LabelPunchDuration = 0.14f, LabelPunchVibrato = 1, LabelPunchElasticity = 0.5f,
            ArrivalAccentInterval = 0.11f, ArrivalParticleCount = 1, FinalArrivalMultiplier = 1.3f,
            CompletionShakeStrength = 0.18f, CompletionShakeDuration = 0.32f, CompletionShakeVibrato = 6,
        };
    }

    // Fast-escalating pitch, expo eases - the most energetic ramp.
    private static BarracksUpgradeZoneAnimationSettings TurboRush()
    {
        return new BarracksUpgradeZoneAnimationSettings
        {
            ShowDuration = 0.14f, ShowOvershootScale = 1.18f, ShowEase = Ease.OutExpo,
            HideDuration = 0.16f, HideEase = Ease.OutExpo,
            FillDuration = 0.16f, FillEase = Ease.OutExpo,
            BaseTossPitch = 1.05f, TossPitchStep = 0.045f, MaxTossPitch = 1.9f,
            LabelPunch = 0.2f, LabelPunchDuration = 0.1f, LabelPunchVibrato = 2, LabelPunchElasticity = 0.7f,
            ArrivalAccentInterval = 0.05f, ArrivalParticleCount = 1, FinalArrivalMultiplier = 1.5f,
            CompletionShakeStrength = 0.4f, CompletionShakeDuration = 0.22f, CompletionShakeVibrato = 16,
        };
    }

    // Big OutBack overshoot, OutBounce hide - playful and toylike.
    private static BarracksUpgradeZoneAnimationSettings CartoonSquash()
    {
        return new BarracksUpgradeZoneAnimationSettings
        {
            ShowDuration = 0.22f, ShowOvershootScale = 1.34f, ShowEase = Ease.OutBack,
            HideDuration = 0.26f, HideEase = Ease.OutBounce,
            FillDuration = 0.24f, FillEase = Ease.OutBack,
            BaseTossPitch = 1f, TossPitchStep = 0.032f, MaxTossPitch = 1.65f,
            LabelPunch = 0.3f, LabelPunchDuration = 0.16f, LabelPunchVibrato = 3, LabelPunchElasticity = 0.85f,
            ArrivalAccentInterval = 0.08f, ArrivalParticleCount = 2, FinalArrivalMultiplier = 1.8f,
            CompletionShakeStrength = 0.5f, CompletionShakeDuration = 0.35f, CompletionShakeVibrato = 10,
        };
    }

    // Moderate pacing throughout, saves the biggest burst and shake for the finish.
    private static BarracksUpgradeZoneAnimationSettings FireworkFinish()
    {
        return new BarracksUpgradeZoneAnimationSettings
        {
            ShowDuration = 0.2f, ShowOvershootScale = 1.22f, ShowEase = Ease.OutBack,
            HideDuration = 0.2f, HideEase = Ease.Linear,
            FillDuration = 0.22f, FillEase = Ease.OutBack,
            BaseTossPitch = 1f, TossPitchStep = 0.03f, MaxTossPitch = 1.6f,
            LabelPunch = 0.2f, LabelPunchDuration = 0.13f, LabelPunchVibrato = 2, LabelPunchElasticity = 0.8f,
            ArrivalAccentInterval = 0.09f, ArrivalParticleCount = 1, FinalArrivalMultiplier = 2.2f,
            CompletionShakeStrength = 0.7f, CompletionShakeDuration = 0.42f, CompletionShakeVibrato = 12,
        };
    }
}
