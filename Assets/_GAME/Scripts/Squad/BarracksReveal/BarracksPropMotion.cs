using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class BarracksPropMotion
{
    [Header("Spawn (appears already airborne, scale 0 -> overshoot, fixed point)")]
    [Min(0f)] public float AirHeight = 0.5f;           // height above the resting spot where it first appears
    [Min(0f)] public float RevealStartTime = 0f;       // offset from apex start
    [Min(0f)] public float RevealStagger = 0.06f;      // delay between each prop in this group
    [Min(0.01f)] public float RevealDuration = 0.18f;  // scale 0 -> 1, OutBack overshoots on its own

    [Header("Air hop (small bounce, back to the same air height)")]
    [Min(0f)] public float HopHeight = 0.12f;
    [Min(0.01f)] public float HopDuration = 0.2f;
    [Min(1f)] public float HopStretch = 1.15f;         // squash & stretch pulse through the hop
    [Range(0f, 45f)] public float HopSpinAngle = 10f;  // springy rotation, peaks at the hop's highest point
    [Min(0f)] public float HopSpinCycles = 1.5f;       // oscillation count for the elastic/bounce feel
    public Vector3 HopSpinAxis = Vector3.up;           // local axis to spin around (normalized at use)

    [Header("Fall & land (staggered after the building lands, or after the hop, whichever is later)")]
    [Min(0f)] public float LandStagger = 0.05f;
    [Min(0.01f)] public float FallDuration = 0.15f;
    public Ease FallEase = Ease.InQuad;
    public Vector3 LandSquash = new Vector3(1.15f, 0.82f, 1.15f);
    [Min(0.01f)] public float LandSettleDuration = 0.14f;

    [Header("Dust / Audio")]
    [Min(0)] public int DustCount = 3;
    [Min(0f)] public float DustRadius = 0.3f;
    public SfxId LandSfx = SfxId.BarracksPropLand;
    [Range(0.1f, 3f)] public float LandPitch = 1f;
}
