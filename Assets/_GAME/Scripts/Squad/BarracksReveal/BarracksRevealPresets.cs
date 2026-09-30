using DG.Tweening;
using UnityEngine;

public static class BarracksRevealPresets
{
    public static BarracksRevealSettings Create(BarracksRevealPreset preset)
    {
        switch (preset)
        {
            case BarracksRevealPreset.SnappyArcade: return SnappyArcade();
            case BarracksRevealPreset.QuickPunch: return QuickPunch();
            case BarracksRevealPreset.JuicyBounce: return JuicyBounce();
            case BarracksRevealPreset.PlayfulCartoon: return PlayfulCartoon();
            case BarracksRevealPreset.HeavyIndustrial: return HeavyIndustrial();
            case BarracksRevealPreset.SubtleRefined: return SubtleRefined();
            case BarracksRevealPreset.PopcornCascade: return PopcornCascade();
            case BarracksRevealPreset.FloatyDream: return FloatyDream();
            default: return Balanced();
        }
    }

    // Mid-weight readable default - matches the tuned look already approved. 1.26s.
    private static BarracksRevealSettings Balanced()
    {
        return new BarracksRevealSettings();
    }

    // Tight and crisp, hypercasual timing. 0.91s.
    private static BarracksRevealSettings SnappyArcade()
    {
        return new BarracksRevealSettings
        {
            AnticipationDuration = 0.1f, AnticipationSquash = new Vector3(1.05f, 0.9f, 1.05f),
            TrembleStrength = 0.04f, TrembleVibrato = 30,
            BuildingJumpUpDuration = 0.12f, BuildingJumpHeight = 0.7f, BuildingJumpUpEase = Ease.OutCubic,
            BuildingLaunchStretch = new Vector3(0.92f, 1.16f, 0.92f),
            BuildingApexDuration = 0.3f, BuildingApexOvershoot = 1.14f,
            BuildingApexSpin = 10f, BuildingApexSpinCycles = 1.5f, BuildingApexSpinAxis = Vector3.up,
            BuildingFallDuration = 0.1f, BuildingFallEase = Ease.InCubic,
            BuildingLandSquash = new Vector3(1.18f, 0.82f, 1.18f), BuildingLandSettleDuration = 0.12f,
            BuildingDustCount = 12, BuildingDustRadius = 1.9f,
            Crates = new BarracksPropMotion
            {
                AirHeight = 0.4f, RevealStartTime = 0f, RevealStagger = 0.045f, RevealDuration = 0.12f,
                HopHeight = 0.1f, HopDuration = 0.14f, HopStretch = 1.14f,
                HopSpinAngle = 12f, HopSpinCycles = 1.5f, HopSpinAxis = Vector3.up,
                LandStagger = 0.04f, FallDuration = 0.11f, FallEase = Ease.InCubic,
                LandSquash = new Vector3(1.16f, 0.82f, 1.16f), LandSettleDuration = 0.11f,
                DustCount = 4, DustRadius = 0.32f, LandPitch = 1.05f,
            },
            Barrels = new BarracksPropMotion
            {
                AirHeight = 0.36f, RevealStartTime = 0.04f, RevealStagger = 0.05f, RevealDuration = 0.13f,
                HopHeight = 0.1f, HopDuration = 0.15f, HopStretch = 1.14f,
                HopSpinAngle = 9f, HopSpinCycles = 1.5f, HopSpinAxis = Vector3.forward,
                LandStagger = 0.04f, FallDuration = 0.11f, FallEase = Ease.InCubic,
                LandSquash = new Vector3(1.12f, 0.86f, 1.12f), LandSettleDuration = 0.11f,
                DustCount = 4, DustRadius = 0.28f, LandPitch = 1.15f,
            },
            Equipment = new BarracksPropMotion
            {
                AirHeight = 0.28f, RevealStartTime = 0.08f, RevealStagger = 0.04f, RevealDuration = 0.14f,
                HopHeight = 0.07f, HopDuration = 0.14f, HopStretch = 1.07f,
                HopSpinAngle = 5f, HopSpinCycles = 1.2f, HopSpinAxis = Vector3.up,
                LandStagger = 0.035f, FallDuration = 0.12f, FallEase = Ease.InCubic,
                LandSquash = new Vector3(1.06f, 0.93f, 1.06f), LandSettleDuration = 0.13f,
                DustCount = 9, DustRadius = 0.8f, LandPitch = 0.8f,
            },
        };
    }

    // Shortest possible read - for upgrades that fire often. 0.63s.
    private static BarracksRevealSettings QuickPunch()
    {
        return new BarracksRevealSettings
        {
            AnticipationDuration = 0.07f, AnticipationSquash = new Vector3(1.04f, 0.92f, 1.04f),
            TrembleStrength = 0.03f, TrembleVibrato = 34,
            BuildingJumpUpDuration = 0.09f, BuildingJumpHeight = 0.45f, BuildingJumpUpEase = Ease.OutCubic,
            BuildingLaunchStretch = new Vector3(0.94f, 1.12f, 0.94f),
            BuildingApexDuration = 0.17f, BuildingApexOvershoot = 1.1f,
            BuildingApexSpin = 7f, BuildingApexSpinCycles = 1f, BuildingApexSpinAxis = Vector3.up,
            BuildingFallDuration = 0.08f, BuildingFallEase = Ease.InCubic,
            BuildingLandSquash = new Vector3(1.16f, 0.84f, 1.16f), BuildingLandSettleDuration = 0.1f,
            BuildingDustCount = 10, BuildingDustRadius = 1.6f,
            Crates = new BarracksPropMotion
            {
                AirHeight = 0.26f, RevealStartTime = 0f, RevealStagger = 0.03f, RevealDuration = 0.09f,
                HopHeight = 0.07f, HopDuration = 0.1f, HopStretch = 1.12f,
                HopSpinAngle = 9f, HopSpinCycles = 1f, HopSpinAxis = Vector3.up,
                LandStagger = 0.03f, FallDuration = 0.08f, FallEase = Ease.InCubic,
                LandSquash = new Vector3(1.14f, 0.85f, 1.14f), LandSettleDuration = 0.09f,
                DustCount = 3, DustRadius = 0.28f, LandPitch = 1.1f,
            },
            Barrels = new BarracksPropMotion
            {
                AirHeight = 0.24f, RevealStartTime = 0.02f, RevealStagger = 0.035f, RevealDuration = 0.1f,
                HopHeight = 0.07f, HopDuration = 0.1f, HopStretch = 1.12f,
                HopSpinAngle = 7f, HopSpinCycles = 1f, HopSpinAxis = Vector3.forward,
                LandStagger = 0.03f, FallDuration = 0.08f, FallEase = Ease.InCubic,
                LandSquash = new Vector3(1.1f, 0.88f, 1.1f), LandSettleDuration = 0.09f,
                DustCount = 3, DustRadius = 0.26f, LandPitch = 1.2f,
            },
            Equipment = new BarracksPropMotion
            {
                AirHeight = 0.2f, RevealStartTime = 0.04f, RevealStagger = 0.03f, RevealDuration = 0.1f,
                HopHeight = 0.05f, HopDuration = 0.09f, HopStretch = 1.05f,
                HopSpinAngle = 4f, HopSpinCycles = 1f, HopSpinAxis = Vector3.up,
                LandStagger = 0.025f, FallDuration = 0.09f, FallEase = Ease.InCubic,
                LandSquash = new Vector3(1.05f, 0.94f, 1.05f), LandSettleDuration = 0.1f,
                DustCount = 7, DustRadius = 0.7f, LandPitch = 0.85f,
            },
        };
    }

    // Everything dialled up - big air, deep squash, loud dust. 1.52s.
    private static BarracksRevealSettings JuicyBounce()
    {
        return new BarracksRevealSettings
        {
            AnticipationDuration = 0.18f, AnticipationSquash = new Vector3(1.1f, 0.82f, 1.1f),
            TrembleStrength = 0.07f, TrembleVibrato = 24,
            BuildingJumpUpDuration = 0.2f, BuildingJumpHeight = 1.35f, BuildingJumpUpEase = Ease.OutQuad,
            BuildingLaunchStretch = new Vector3(0.85f, 1.3f, 0.85f),
            BuildingApexDuration = 0.52f, BuildingApexOvershoot = 1.28f,
            BuildingApexSpin = 18f, BuildingApexSpinCycles = 2f, BuildingApexSpinAxis = Vector3.up,
            BuildingFallDuration = 0.16f, BuildingFallEase = Ease.InQuad,
            BuildingLandSquash = new Vector3(1.34f, 0.68f, 1.34f), BuildingLandSettleDuration = 0.22f,
            BuildingDustCount = 20, BuildingDustRadius = 2.8f,
            Crates = new BarracksPropMotion
            {
                AirHeight = 0.7f, RevealStartTime = 0f, RevealStagger = 0.08f, RevealDuration = 0.2f,
                HopHeight = 0.2f, HopDuration = 0.24f, HopStretch = 1.26f,
                HopSpinAngle = 20f, HopSpinCycles = 2f, HopSpinAxis = Vector3.up,
                LandStagger = 0.07f, FallDuration = 0.17f, FallEase = Ease.InQuad,
                LandSquash = new Vector3(1.28f, 0.72f, 1.28f), LandSettleDuration = 0.2f,
                DustCount = 6, DustRadius = 0.45f, LandPitch = 1f,
            },
            Barrels = new BarracksPropMotion
            {
                AirHeight = 0.62f, RevealStartTime = 0.08f, RevealStagger = 0.09f, RevealDuration = 0.22f,
                HopHeight = 0.2f, HopDuration = 0.26f, HopStretch = 1.26f,
                HopSpinAngle = 16f, HopSpinCycles = 2f, HopSpinAxis = Vector3.forward,
                LandStagger = 0.07f, FallDuration = 0.17f, FallEase = Ease.InQuad,
                LandSquash = new Vector3(1.24f, 0.76f, 1.24f), LandSettleDuration = 0.2f,
                DustCount = 6, DustRadius = 0.4f, LandPitch = 1.15f,
            },
            Equipment = new BarracksPropMotion
            {
                AirHeight = 0.48f, RevealStartTime = 0.16f, RevealStagger = 0.06f, RevealDuration = 0.24f,
                HopHeight = 0.13f, HopDuration = 0.24f, HopStretch = 1.14f,
                HopSpinAngle = 10f, HopSpinCycles = 1.5f, HopSpinAxis = Vector3.up,
                LandStagger = 0.05f, FallDuration = 0.19f, FallEase = Ease.InQuad,
                LandSquash = new Vector3(1.12f, 0.88f, 1.12f), LandSettleDuration = 0.22f,
                DustCount = 14, DustRadius = 1.1f, LandPitch = 0.7f,
            },
        };
    }

    // Exaggerated squash/stretch, wide spins, hangs before dropping. 1.45s.
    private static BarracksRevealSettings PlayfulCartoon()
    {
        return new BarracksRevealSettings
        {
            AnticipationDuration = 0.2f, AnticipationSquash = new Vector3(1.14f, 0.78f, 1.14f),
            TrembleStrength = 0.09f, TrembleVibrato = 28,
            BuildingJumpUpDuration = 0.18f, BuildingJumpHeight = 1.15f, BuildingJumpUpEase = Ease.OutBack,
            BuildingLaunchStretch = new Vector3(0.8f, 1.36f, 0.8f),
            BuildingApexDuration = 0.48f, BuildingApexOvershoot = 1.24f,
            BuildingApexSpin = 26f, BuildingApexSpinCycles = 2.5f,
            BuildingApexSpinAxis = new Vector3(0.25f, 1f, 0.15f),
            BuildingFallDuration = 0.15f, BuildingFallEase = Ease.InBack,
            BuildingLandSquash = new Vector3(1.4f, 0.64f, 1.4f), BuildingLandSettleDuration = 0.2f,
            BuildingDustCount = 18, BuildingDustRadius = 2.6f,
            Crates = new BarracksPropMotion
            {
                AirHeight = 0.62f, RevealStartTime = 0f, RevealStagger = 0.075f, RevealDuration = 0.19f,
                HopHeight = 0.18f, HopDuration = 0.24f, HopStretch = 1.3f,
                HopSpinAngle = 30f, HopSpinCycles = 2.5f, HopSpinAxis = new Vector3(0.2f, 1f, 0.3f),
                LandStagger = 0.06f, FallDuration = 0.16f, FallEase = Ease.InBack,
                LandSquash = new Vector3(1.34f, 0.68f, 1.34f), LandSettleDuration = 0.19f,
                DustCount = 5, DustRadius = 0.42f, LandPitch = 1.1f,
            },
            Barrels = new BarracksPropMotion
            {
                AirHeight = 0.56f, RevealStartTime = 0.07f, RevealStagger = 0.085f, RevealDuration = 0.21f,
                HopHeight = 0.18f, HopDuration = 0.26f, HopStretch = 1.3f,
                HopSpinAngle = 24f, HopSpinCycles = 3f, HopSpinAxis = Vector3.forward,
                LandStagger = 0.06f, FallDuration = 0.16f, FallEase = Ease.InBack,
                LandSquash = new Vector3(1.28f, 0.74f, 1.28f), LandSettleDuration = 0.19f,
                DustCount = 5, DustRadius = 0.38f, LandPitch = 1.25f,
            },
            Equipment = new BarracksPropMotion
            {
                AirHeight = 0.42f, RevealStartTime = 0.14f, RevealStagger = 0.06f, RevealDuration = 0.23f,
                HopHeight = 0.11f, HopDuration = 0.22f, HopStretch = 1.16f,
                HopSpinAngle = 14f, HopSpinCycles = 2f, HopSpinAxis = Vector3.right,
                LandStagger = 0.05f, FallDuration = 0.18f, FallEase = Ease.InQuad,
                LandSquash = new Vector3(1.14f, 0.86f, 1.14f), LandSettleDuration = 0.21f,
                DustCount = 13, DustRadius = 1f, LandPitch = 0.7f,
            },
        };
    }

    // Low, slow and heavy - things slam rather than bounce. 1.58s.
    private static BarracksRevealSettings HeavyIndustrial()
    {
        return new BarracksRevealSettings
        {
            AnticipationDuration = 0.22f, AnticipationSquash = new Vector3(1.08f, 0.86f, 1.08f),
            TrembleStrength = 0.08f, TrembleVibrato = 18,
            BuildingJumpUpDuration = 0.22f, BuildingJumpHeight = 0.65f, BuildingJumpUpEase = Ease.OutSine,
            BuildingLaunchStretch = new Vector3(0.94f, 1.1f, 0.94f),
            BuildingApexDuration = 0.42f, BuildingApexOvershoot = 1.08f,
            BuildingApexSpin = 6f, BuildingApexSpinCycles = 1f, BuildingApexSpinAxis = Vector3.up,
            BuildingFallDuration = 0.2f, BuildingFallEase = Ease.InQuart,
            BuildingLandSquash = new Vector3(1.3f, 0.7f, 1.3f), BuildingLandSettleDuration = 0.26f,
            BuildingDustCount = 26, BuildingDustRadius = 3.2f,
            Crates = new BarracksPropMotion
            {
                AirHeight = 0.42f, RevealStartTime = 0f, RevealStagger = 0.09f, RevealDuration = 0.2f,
                HopHeight = 0.07f, HopDuration = 0.2f, HopStretch = 1.08f,
                HopSpinAngle = 7f, HopSpinCycles = 1f, HopSpinAxis = Vector3.up,
                LandStagger = 0.08f, FallDuration = 0.2f, FallEase = Ease.InQuart,
                LandSquash = new Vector3(1.24f, 0.76f, 1.24f), LandSettleDuration = 0.22f,
                DustCount = 7, DustRadius = 0.5f, LandPitch = 0.8f,
            },
            Barrels = new BarracksPropMotion
            {
                AirHeight = 0.38f, RevealStartTime = 0.08f, RevealStagger = 0.1f, RevealDuration = 0.22f,
                HopHeight = 0.07f, HopDuration = 0.22f, HopStretch = 1.08f,
                HopSpinAngle = 6f, HopSpinCycles = 1f, HopSpinAxis = Vector3.forward,
                LandStagger = 0.08f, FallDuration = 0.2f, FallEase = Ease.InQuart,
                LandSquash = new Vector3(1.2f, 0.8f, 1.2f), LandSettleDuration = 0.22f,
                DustCount = 7, DustRadius = 0.45f, LandPitch = 0.9f,
            },
            Equipment = new BarracksPropMotion
            {
                AirHeight = 0.3f, RevealStartTime = 0.16f, RevealStagger = 0.07f, RevealDuration = 0.24f,
                HopHeight = 0.05f, HopDuration = 0.2f, HopStretch = 1.04f,
                HopSpinAngle = 4f, HopSpinCycles = 1f, HopSpinAxis = Vector3.up,
                LandStagger = 0.06f, FallDuration = 0.22f, FallEase = Ease.InQuart,
                LandSquash = new Vector3(1.1f, 0.9f, 1.1f), LandSettleDuration = 0.24f,
                DustCount = 16, DustRadius = 1.2f, LandPitch = 0.6f,
            },
        };
    }

    // Restrained and clean - reads as polish, not cartoon. 1.07s.
    private static BarracksRevealSettings SubtleRefined()
    {
        return new BarracksRevealSettings
        {
            AnticipationDuration = 0.12f, AnticipationSquash = new Vector3(1.03f, 0.94f, 1.03f),
            TrembleStrength = 0.025f, TrembleVibrato = 22,
            BuildingJumpUpDuration = 0.14f, BuildingJumpHeight = 0.5f, BuildingJumpUpEase = Ease.OutSine,
            BuildingLaunchStretch = new Vector3(0.95f, 1.09f, 0.95f),
            BuildingApexDuration = 0.34f, BuildingApexOvershoot = 1.07f,
            BuildingApexSpin = 5f, BuildingApexSpinCycles = 1f, BuildingApexSpinAxis = Vector3.up,
            BuildingFallDuration = 0.13f, BuildingFallEase = Ease.InSine,
            BuildingLandSquash = new Vector3(1.1f, 0.9f, 1.1f), BuildingLandSettleDuration = 0.15f,
            BuildingDustCount = 8, BuildingDustRadius = 1.7f,
            Crates = new BarracksPropMotion
            {
                AirHeight = 0.3f, RevealStartTime = 0f, RevealStagger = 0.06f, RevealDuration = 0.16f,
                HopHeight = 0.06f, HopDuration = 0.16f, HopStretch = 1.06f,
                HopSpinAngle = 5f, HopSpinCycles = 1f, HopSpinAxis = Vector3.up,
                LandStagger = 0.05f, FallDuration = 0.14f, FallEase = Ease.InSine,
                LandSquash = new Vector3(1.08f, 0.92f, 1.08f), LandSettleDuration = 0.14f,
                DustCount = 2, DustRadius = 0.26f, LandPitch = 1f,
            },
            Barrels = new BarracksPropMotion
            {
                AirHeight = 0.28f, RevealStartTime = 0.05f, RevealStagger = 0.065f, RevealDuration = 0.17f,
                HopHeight = 0.06f, HopDuration = 0.17f, HopStretch = 1.06f,
                HopSpinAngle = 4f, HopSpinCycles = 1f, HopSpinAxis = Vector3.forward,
                LandStagger = 0.05f, FallDuration = 0.14f, FallEase = Ease.InSine,
                LandSquash = new Vector3(1.07f, 0.93f, 1.07f), LandSettleDuration = 0.14f,
                DustCount = 2, DustRadius = 0.24f, LandPitch = 1.1f,
            },
            Equipment = new BarracksPropMotion
            {
                AirHeight = 0.22f, RevealStartTime = 0.1f, RevealStagger = 0.05f, RevealDuration = 0.18f,
                HopHeight = 0.04f, HopDuration = 0.15f, HopStretch = 1.03f,
                HopSpinAngle = 3f, HopSpinCycles = 1f, HopSpinAxis = Vector3.up,
                LandStagger = 0.04f, FallDuration = 0.15f, FallEase = Ease.InSine,
                LandSquash = new Vector3(1.04f, 0.96f, 1.04f), LandSettleDuration = 0.15f,
                DustCount = 6, DustRadius = 0.7f, LandPitch = 0.85f,
            },
        };
    }

    // Long hang time, props pop one at a time instead of as a chord. 1.64s.
    private static BarracksRevealSettings PopcornCascade()
    {
        return new BarracksRevealSettings
        {
            AnticipationDuration = 0.14f, AnticipationSquash = new Vector3(1.06f, 0.89f, 1.06f),
            TrembleStrength = 0.05f, TrembleVibrato = 26,
            BuildingJumpUpDuration = 0.15f, BuildingJumpHeight = 0.8f, BuildingJumpUpEase = Ease.OutQuad,
            BuildingLaunchStretch = new Vector3(0.9f, 1.18f, 0.9f),
            BuildingApexDuration = 0.52f, BuildingApexOvershoot = 1.16f,
            BuildingApexSpin = 11f, BuildingApexSpinCycles = 1.5f, BuildingApexSpinAxis = Vector3.up,
            BuildingFallDuration = 0.14f, BuildingFallEase = Ease.InQuad,
            BuildingLandSquash = new Vector3(1.22f, 0.78f, 1.22f), BuildingLandSettleDuration = 0.16f,
            BuildingDustCount = 14, BuildingDustRadius = 2.2f,
            Crates = new BarracksPropMotion
            {
                AirHeight = 0.52f, RevealStartTime = 0f, RevealStagger = 0.16f, RevealDuration = 0.16f,
                HopHeight = 0.16f, HopDuration = 0.2f, HopStretch = 1.2f,
                HopSpinAngle = 16f, HopSpinCycles = 1.5f, HopSpinAxis = Vector3.up,
                LandStagger = 0.12f, FallDuration = 0.15f, FallEase = Ease.InQuad,
                LandSquash = new Vector3(1.18f, 0.8f, 1.18f), LandSettleDuration = 0.15f,
                DustCount = 5, DustRadius = 0.38f, LandPitch = 1f,
            },
            Barrels = new BarracksPropMotion
            {
                AirHeight = 0.48f, RevealStartTime = 0.22f, RevealStagger = 0.17f, RevealDuration = 0.17f,
                HopHeight = 0.16f, HopDuration = 0.21f, HopStretch = 1.2f,
                HopSpinAngle = 13f, HopSpinCycles = 1.5f, HopSpinAxis = Vector3.forward,
                LandStagger = 0.12f, FallDuration = 0.15f, FallEase = Ease.InQuad,
                LandSquash = new Vector3(1.15f, 0.83f, 1.15f), LandSettleDuration = 0.15f,
                DustCount = 5, DustRadius = 0.34f, LandPitch = 1.15f,
            },
            Equipment = new BarracksPropMotion
            {
                AirHeight = 0.38f, RevealStartTime = 0.46f, RevealStagger = 0.14f, RevealDuration = 0.19f,
                HopHeight = 0.11f, HopDuration = 0.22f, HopStretch = 1.1f,
                HopSpinAngle = 8f, HopSpinCycles = 1.2f, HopSpinAxis = Vector3.up,
                LandStagger = 0.1f, FallDuration = 0.17f, FallEase = Ease.InQuad,
                LandSquash = new Vector3(1.08f, 0.91f, 1.08f), LandSettleDuration = 0.17f,
                DustCount = 11, DustRadius = 0.95f, LandPitch = 0.75f,
            },
        };
    }

    // Slow, soft, low gravity - everything drifts down. 2.18s.
    private static BarracksRevealSettings FloatyDream()
    {
        return new BarracksRevealSettings
        {
            AnticipationDuration = 0.24f, AnticipationSquash = new Vector3(1.05f, 0.9f, 1.05f),
            TrembleStrength = 0.03f, TrembleVibrato = 14,
            BuildingJumpUpDuration = 0.3f, BuildingJumpHeight = 1.1f, BuildingJumpUpEase = Ease.OutSine,
            BuildingLaunchStretch = new Vector3(0.92f, 1.14f, 0.92f),
            BuildingApexDuration = 0.65f, BuildingApexOvershoot = 1.12f,
            BuildingApexSpin = 9f, BuildingApexSpinCycles = 1f, BuildingApexSpinAxis = Vector3.up,
            BuildingFallDuration = 0.32f, BuildingFallEase = Ease.InSine,
            BuildingLandSquash = new Vector3(1.14f, 0.87f, 1.14f), BuildingLandSettleDuration = 0.28f,
            BuildingDustCount = 10, BuildingDustRadius = 2.4f,
            Crates = new BarracksPropMotion
            {
                AirHeight = 0.6f, RevealStartTime = 0f, RevealStagger = 0.1f, RevealDuration = 0.26f,
                HopHeight = 0.14f, HopDuration = 0.32f, HopStretch = 1.12f,
                HopSpinAngle = 10f, HopSpinCycles = 1f, HopSpinAxis = Vector3.up,
                LandStagger = 0.09f, FallDuration = 0.3f, FallEase = Ease.InSine,
                LandSquash = new Vector3(1.12f, 0.88f, 1.12f), LandSettleDuration = 0.26f,
                DustCount = 3, DustRadius = 0.4f, LandPitch = 0.9f,
            },
            Barrels = new BarracksPropMotion
            {
                AirHeight = 0.55f, RevealStartTime = 0.1f, RevealStagger = 0.11f, RevealDuration = 0.28f,
                HopHeight = 0.14f, HopDuration = 0.34f, HopStretch = 1.12f,
                HopSpinAngle = 8f, HopSpinCycles = 1f, HopSpinAxis = Vector3.forward,
                LandStagger = 0.09f, FallDuration = 0.3f, FallEase = Ease.InSine,
                LandSquash = new Vector3(1.1f, 0.9f, 1.1f), LandSettleDuration = 0.26f,
                DustCount = 3, DustRadius = 0.36f, LandPitch = 1f,
            },
            Equipment = new BarracksPropMotion
            {
                AirHeight = 0.44f, RevealStartTime = 0.2f, RevealStagger = 0.09f, RevealDuration = 0.3f,
                HopHeight = 0.1f, HopDuration = 0.32f, HopStretch = 1.06f,
                HopSpinAngle = 6f, HopSpinCycles = 1f, HopSpinAxis = Vector3.up,
                LandStagger = 0.07f, FallDuration = 0.32f, FallEase = Ease.InSine,
                LandSquash = new Vector3(1.05f, 0.95f, 1.05f), LandSettleDuration = 0.28f,
                DustCount = 9, DustRadius = 0.95f, LandPitch = 0.7f,
            },
        };
    }
}
