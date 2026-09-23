using UnityEngine;

// Single data-driven source of truth for camera shake: global limits plus the
// per-trigger shake shapes. Pure data - the mapping from a gameplay event to a
// shake scale lives in the listener that owns that event.
[CreateAssetMenu(fileName = "CameraShakeConfig", menuName = "Config/CameraShakeConfig")]
public class CameraShakeConfig : ScriptableObject
{
    [Header("Global")]
    [Tooltip("Master off switch. Useful for an accessibility setting or for profiling.")]
    public bool Enabled = true;

    [Tooltip("Scales every shake in the game. 0 disables, 1 is authored strength.")]
    [Range(0f, 2f)] public float MasterAmplitude = 1f;

    [Tooltip("Shakes that overlap are summed, so an unbounded stack turns into mush. " +
             "Adding past this count drops the weakest live shake first.")]
    [Range(1, 8)] public int MaxConcurrentShakes = 3;

    [Tooltip("Run the shake on unscaled time so it keeps its authored length during " +
             "a hit-stop or slow-motion moment.")]
    public bool UseUnscaledTime = true;

    [Header("Hero Damage")]
    public CameraShakeSettings HeroDamage = new CameraShakeSettings
    {
        Duration = 0.10f,
        PositionAmplitude = 0.06f,
        RotationAmplitude = 0.25f,
        Frequency = 24f,
        DirectionalPunch = 0.015f,
        DepthInfluence = 0.1f,
    };

    [Tooltip("Damage hits arriving sooner than this after the previous one are ignored. " +
             "Stops a fast multi-enemy burst from becoming one continuous rattle.")]
    [Min(0f)] public float HeroDamageMinInterval = 0.15f;

    [Tooltip("Minimum random multiplier of the Hero Damage position, rotation and directional punch. " +
             "A new strength is sampled for each hit accepted by the cooldown; 1 is authored strength.")]
    [Min(0f)] public float HeroDamageStrengthMin = 0.75f;

    [Tooltip("Maximum random multiplier of the Hero Damage amplitudes. " +
             "Set equal to the minimum for a fixed strength.")]
    [Min(0f)] public float HeroDamageStrengthMax = 1.25f;

    [Tooltip("Push the camera away from the attacker. Off makes the shake purely " +
             "omnidirectional, which hides where the damage came from.")]
    public bool UseHitDirection = true;

    [Header("Enemy Kill")]
    [Tooltip("Fires on every enemy death. Authored small on purpose - it happens " +
             "dozens of times a minute, so it is a tick, not a jolt.")]
    public CameraShakeSettings EnemyKill = new CameraShakeSettings
    {
        Duration = 0.12f,
        PositionAmplitude = 0.06f,
        RotationAmplitude = 0.35f,
        Frequency = 30f,
        DirectionalPunch = 0f,
        DepthInfluence = 0.15f,
    };

    [Tooltip("Kills landing sooner than this after the previous shake are " +
             "ignored, so a wipe does not turn into one long rattle.")]
    [Min(0f)] public float EnemyKillMinInterval = 0.08f;

    [Header("Squad Clash")]
    [Tooltip("The moment the formation reaches the enemy group. Once per fight, " +
             "so it can afford to be heavy.")]
    public CameraShakeSettings SquadClash = new CameraShakeSettings
    {
        Duration = 0.35f,
        PositionAmplitude = 0.22f,
        RotationAmplitude = 1.1f,
        Frequency = 18f,
        DirectionalPunch = 0f,
        DepthInfluence = 0.3f,
    };

    [Header("Capture")]
    [Tooltip("The payoff at the end of the run. The biggest shake in the game.")]
    public CameraShakeSettings CaptureComplete = new CameraShakeSettings
    {
        Duration = 0.5f,
        PositionAmplitude = 0.3f,
        RotationAmplitude = 1.4f,
        Frequency = 14f,
        DirectionalPunch = 0f,
        DepthInfluence = 0.35f,
    };
}
