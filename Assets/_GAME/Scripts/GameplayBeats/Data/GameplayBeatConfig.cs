using UnityEngine;

[CreateAssetMenu(fileName = "GameplayBeatConfig", menuName = "Configs/Gameplay Beat Config")]
public class GameplayBeatConfig : ScriptableObject
{
    [field: SerializeField] public string BattleStartedText { get; private set; } = "MOVE OUT";
    [field: SerializeField] public string EncounterStartedText { get; private set; } = "ENGAGE";
    [field: SerializeField] public string LevelClearedText { get; private set; } = "AREA CLEAR";
    [field: SerializeField] public string LevelCapturedText { get; private set; } = "CAPTURED";

    [field: SerializeField] public Color BattleStartedColor { get; private set; } = new(1f, 0.72f, 0.2f);
    [field: SerializeField] public Color EncounterStartedColor { get; private set; } = new(1f, 0.38f, 0.18f);
    [field: SerializeField] public Color LevelClearedColor { get; private set; } = new(0.35f, 1f, 0.68f);
    [field: SerializeField] public Color LevelCapturedColor { get; private set; } = new(1f, 0.86f, 0.22f);

    [field: SerializeField, Min(0f)] public float BattleStartedHoldDuration { get; private set; } = 0.55f;
    [field: SerializeField, Min(0f)] public float EncounterStartedHoldDuration { get; private set; } = 0.45f;
    [field: SerializeField, Min(0f)] public float LevelClearedHoldDuration { get; private set; } = 1.1f;
    [field: SerializeField, Min(0f)] public float LevelCapturedHoldDuration { get; private set; } = 0.7f;

    [field: SerializeField, Min(0.01f)] public float EnterDuration { get; private set; } = 0.18f;
    [field: SerializeField, Min(0.01f)] public float SettleDuration { get; private set; } = 0.12f;
    [field: SerializeField, Min(0.01f)] public float ExitDuration { get; private set; } = 0.28f;
    [field: SerializeField, Min(0f)] public float EnterOffset { get; private set; } = 70f;
    [field: SerializeField, Range(0.1f, 1f)] public float EnterScale { get; private set; } = 0.7f;
    [field: SerializeField, Min(1f)] public float OvershootScale { get; private set; } = 1.08f;
}
