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
    public float BorderWidth = 0.15f;
    public Material BorderMaterial;

    [Header("Animation")]
    public float FadeInDuration = 0.35f;
    public float FadeOutDuration = 0.6f;
}
