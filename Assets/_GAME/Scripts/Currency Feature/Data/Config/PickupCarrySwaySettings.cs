using System;
using UnityEngine;

[Serializable]
public class PickupCarrySwaySettings
{
    private const float BaseFrequency     = 11f;
    private const float BaseLeanPerSpeed  = 0.06f;
    private const float BaseLeanPerAccel  = 0.012f;
    private const float BaseMaxLeanRatio  = 0.45f;
    private const float DampingRatio      = 1f;
    private const float LinkClearanceSlot = 0.8f;
    private const float TiltPerBend       = 700f;
    private const float TiltCeiling       = 45f;

    [SerializeField] private bool enabled = true;

    [Tooltip("How far the top of the stack leans while the hero moves. Scales with stack height, so the lean angle reads the same at 5 coins and at 40.")]
    [SerializeField, Range(0f, 2f)] private float amount = 1f;

    [Tooltip("How quickly the lean builds and returns. Below 1 feels heavy and lags behind, above 1 follows the hero closely.")]
    [SerializeField, Range(0.25f, 2f)] private float speed = 1f;

    [Tooltip("How quickly the stack straightens up once the hero stops. Independent of the lean speed, so the coins can ease out and snap back.")]
    [SerializeField, Range(0.25f, 4f)] private float returnSpeed = 2f;

    [Tooltip("Share of the stack, measured from the top, that leans at all. The coins below stay put under the weight.")]
    [SerializeField, Range(0.1f, 1f)] private float topShare = 0.34f;

    private float Frequency       => BaseFrequency * speed;
    private float ReturnFrequency => BaseFrequency * returnSpeed;

    public bool  Enabled             => enabled;
    public float TopShare            => topShare;
    public float LeanPerSpeed        => BaseLeanPerSpeed * amount;
    public float LeanPerAcceleration => BaseLeanPerAccel * amount;
    public float MaxLeanRatio        => BaseMaxLeanRatio * Mathf.Max(0.25f, amount);
    public float Stiffness           => Frequency * Frequency;
    public float Damping             => 2f * DampingRatio * Frequency;
    public float ReturnStiffness     => ReturnFrequency * ReturnFrequency;
    public float ReturnDamping       => 2f * DampingRatio * ReturnFrequency;
    public float LinkClearance       => LinkClearanceSlot;
    public float TiltPerUnitBend     => TiltPerBend;
    public float MaxTiltDegrees      => Mathf.Clamp(TiltCeiling * amount, 15f, 70f);
    public float VelocitySmoothing   => 0.35f;
    public float VisualSmoothing     => 0.015f;
}
