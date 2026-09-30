using System;
using UnityEngine;

[Serializable]
public class PickupStackImpactSettings
{
    [SerializeField] private bool enabled = true;

    [Tooltip("Normalized impulse over its lifetime. Starts at 1 (compressed) and springs back through zero.")]
    [SerializeField] private AnimationCurve response = new AnimationCurve(
        new Keyframe(0f, 1f), new Keyframe(0.35f, -0.3f), new Keyframe(0.7f, 0.1f), new Keyframe(1f, 0f));

    [SerializeField, Min(0f)] private float duration = 0.35f;

    [Tooltip("Compression of a single slot. Every slot above it rides down on the sum, so the whole column squashes.")]
    [SerializeField, Min(0f)] private float slotCompression = 0.045f;

    [Tooltip("How much wider the coin face gets at full impulse. This is the part a top-down camera actually sees.")]
    [SerializeField, Range(0f, 1f)] private float faceExpand = 0.3f;

    [Tooltip("How much thinner the coin gets at full impulse.")]
    [SerializeField, Range(0f, 0.9f)] private float flatten = 0.45f;

    [Tooltip("Extra size the coin pops to while it springs back out of the squash, on top of the squash and stretch itself.")]
    [SerializeField, Range(0f, 0.6f)] private float overshoot = 0.2f;

    [SerializeField, Min(0f)] private float tiltDegrees = 22f;

    [Tooltip("Sideways kick handed to the whole carried column, so the stack visibly rocks on every landing.")]
    [SerializeField, Min(0f)] private float columnKick = 0.6f;

    [Tooltip("Extra delay per slot away from the landing, so the jolt travels through the stack.")]
    [SerializeField, Min(0f)] private float propagationDelay = 0.025f;

    [Tooltip("Impulse multiplier applied once per slot away from the landing.")]
    [SerializeField, Range(0f, 1f)] private float falloff = 0.7f;

    [SerializeField, Min(0)] private int maxAffectedNeighbours = 8;

    [SerializeField] private bool playLandingFx = true;

    public bool           Enabled               => enabled;
    public AnimationCurve Response              => response;
    public float          Duration              => Mathf.Max(0.0001f, duration);
    public float          SlotCompression       => slotCompression;
    public float          FaceExpand            => faceExpand;
    public float          Flatten               => flatten;
    public float          Overshoot             => overshoot;
    public float          TiltDegrees           => tiltDegrees;
    public float          ColumnKick            => columnKick;
    public float          PropagationDelay      => propagationDelay;
    public float          Falloff               => falloff;
    public int            MaxAffectedNeighbours => maxAffectedNeighbours;
    public bool           PlayLandingFx         => playLandingFx;
}
