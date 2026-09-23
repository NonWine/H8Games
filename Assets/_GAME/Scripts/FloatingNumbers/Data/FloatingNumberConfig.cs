using UnityEngine;

[CreateAssetMenu(fileName = "FloatingNumberConfig", menuName = "Configs/Floating Number Config")]
public class FloatingNumberConfig : ScriptableObject
{
    [field: SerializeField] public Color EnemyDamageColor { get; private set; } = new(1f, 0.78f, 0.18f);
    [field: SerializeField] public Color FriendlyDamageColor { get; private set; } = new(1f, 0.28f, 0.24f);
    [field: SerializeField] public Color CoinColor { get; private set; } = new(1f, 0.85f, 0.2f);
    [field: SerializeField] public Vector3 DamageOffset { get; private set; } = new(0f, 2.2f, 0f);
    [field: SerializeField] public Vector3 CoinOffset { get; private set; } = new(0f, 1.2f, 0f);
    [field: SerializeField, Min(0f)] public float LateralJitter { get; private set; } = 0.32f;
    [field: SerializeField, Min(0.01f)] public float Duration { get; private set; } = 0.85f;
    [field: SerializeField, Min(0f)] public float RiseDistance { get; private set; } = 1.7f;
    [field: SerializeField, Min(0.1f)] public float DamageFontSize { get; private set; } = 8f;
    [field: SerializeField, Min(0.1f)] public float CoinFontSize { get; private set; } = 7f;
    [field: SerializeField, Min(1f)] public float LethalScale { get; private set; } = 1.25f;
}
