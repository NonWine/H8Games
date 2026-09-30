using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "CinematicFocusConfig", menuName = "Configs/Cinematic Focus Config")]
public class CinematicFocusConfig : ScriptableObject
{
    [field: Header("Global")]
    [field: SerializeField] public bool Enabled { get; private set; } = true;

    [field: Header("Blend")]
    [field: SerializeField, Min(0.01f)] public float FocusInDuration { get; private set; } = 0.55f;
    [field: SerializeField] public Ease FocusInEase { get; private set; } = Ease.InOutSine;
    [field: SerializeField, Min(0.01f)] public float ReleaseDuration { get; private set; } = 0.7f;
    [field: SerializeField] public Ease ReleaseEase { get; private set; } = Ease.InOutSine;

    [field: Header("Framing")]
    [field: SerializeField, Range(0.2f, 2f)] public float DistanceScale { get; private set; } = 0.78f;
    [field: SerializeField, Range(-5f, 10f)] public float HeightOffset { get; private set; } = 1.2f;
    [field: SerializeField, Range(10f, 120f)] public float FieldOfView { get; private set; } = 46f;

    [field: Header("Drift")]
    [field: SerializeField, Range(-45f, 45f)] public float OrbitDegrees { get; private set; } = 8f;
    [field: SerializeField, Min(0.01f)] public float OrbitDuration { get; private set; } = 2.5f;
    [field: SerializeField] public Ease OrbitEase { get; private set; } = Ease.OutSine;
}
