using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "FloatingTextConfig", menuName = "Config/FloatingTextConfig")]
public class FloatingTextConfig : ScriptableObject
{
    public bool Enabled = true;

    [Header("Pool")]
    [Range(4, 64)] public int PoolSize = 24;

    [Header("Motion")]
    [Min(0f)] public float RiseDistance = 1.6f;
    [Min(0.05f)] public float Duration = 0.75f;
    [Min(0f)] public float HorizontalSpread = 0.35f;
    [Min(0f)] public float SpawnHeight = 1.8f;
    public Ease RiseEase = Ease.OutCubic;

    [Header("Scale")]
    [Min(0.01f)] public float StartScale = 0.4f;
    [Min(0.01f)] public float PeakScale = 1f;
    [Range(0f, 1f)] public float PeakScaleTime = 0.25f;

    [Header("Fade")]
    [Range(0f, 1f)] public float FadeStartTime = 0.55f;

    [Header("Colors")]
    public Color EnemyDamageColor = new Color(1f, 0.95f, 0.65f);
    public Color AllyDamageColor = new Color(1f, 0.38f, 0.34f);
    public Color CoinColor = new Color(1f, 0.82f, 0.25f);

    [Header("Rate limiting")]
    [Tooltip("Damage numbers below this value are not shown at all.")]
    [Min(0f)] public float MinDamageToShow = 1f;

    [Min(0f)] public float MinIntervalSeconds = 0.03f;
}
