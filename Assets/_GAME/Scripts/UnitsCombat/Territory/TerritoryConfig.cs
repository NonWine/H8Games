using UnityEngine;

[CreateAssetMenu(fileName = "TerritoryConfig", menuName = "Config/TerritoryConfig")]
public class TerritoryConfig : ScriptableObject
{
    [Header("Tracking")]
    public float UpdateInterval = 0.15f;

    [Header("Smoothing")]
    public float ExpandDuration = 0.25f;
    public float ShrinkDuration = 0.4f;
    public float SnapDistance   = 0.05f;

    [Header("Shape")]
    public float MinRadius = 2f;
    public float Padding = 1f;
    public int CornerSegments = 5;
    public int CircleSegments = 24;
    public float GroundY = 0f;
    public float VerticalOffset = 0.02f;

    [Header("Border")]
    public float    BorderWidth        = 0.15f;
    public int      BorderResampleCount = 48;
    public float    BorderYOffset      = 0.01f;   // lift above fill mesh to avoid z-fighting
    public Material BorderMaterial;

    [Header("Animation")]
    public float FadeInDuration  = 0.35f;
    public float FadeOutDuration = 0.6f;
    public float FillAlpha       = 0.4f;

    [Header("Zone Static")]
    public Color StaticFillColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    [Header("Zone Pulse")]
    public Color PulseColorA = new Color(0.80f, 0f, 0f, 1f);
    public Color PulseColorB = new Color(1f, 0.33f, 0f, 1f);
    public float PulseFrequency = 2.5f;

    [Header("Border Particles")]
    public float BorderParticlesPerSecond = 15f;
    public float BorderParticleSpeed = 1.5f;
    public float BorderParticleLifetime = 0.8f;
    public float BorderParticleSize = 0.12f;

    [Header("Combat Alert")]
    public float CombatPulseFrequency = 5f;
    public float CombatBrightnessMult = 1.4f;
    public int   CombatBurstCount = 40;

    [Header("Danger Camera FX")]
    public float DangerVignetteIntensity = 0.35f;
    public float DangerChromaticAberration = 0.4f;
    public float CameraShakeAmplitude = 0.02f;
    public float CameraShakeFrequency = 15f;
    public float DangerFXDuration = 0.4f;
}
