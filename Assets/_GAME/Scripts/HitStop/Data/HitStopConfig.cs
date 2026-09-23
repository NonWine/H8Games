using UnityEngine;

// Data-driven hit-stop. Every number here is felt rather than read, so they all
// live in one asset that can be tuned while the game runs.
[CreateAssetMenu(fileName = "HitStopConfig", menuName = "Config/HitStopConfig")]
public class HitStopConfig : ScriptableObject
{
    [Header("Global")]
    [Tooltip("Master off switch - useful when recording footage that must not " +
             "stutter, and for profiling.")]
    public bool Enabled = true;

    [Header("Enemy kill")]
    [Tooltip("Time scale held during the freeze. Not 0: a dead stop reads as a " +
             "hitch, while a crawl reads as impact.")]
    [Range(0f, 1f)] public float KillTimeScale = 0.05f;

    [Tooltip("Real seconds the freeze lasts. Past about 0.09 it stops feeling " +
             "like a hit and starts feeling like a frame drop.")]
    [Range(0f, 0.3f)] public float KillDuration = 0.07f;

    [Tooltip("Kills landing sooner than this after the last freeze are ignored. " +
             "A squad wiping a group would otherwise chain freezes into slow " +
             "motion that never ends.")]
    [Min(0f)] public float KillMinInterval = 0.25f;

    [Header("Hero defeat")]
    [Range(0f, 1f)] public float HeroDefeatTimeScale = 0.15f;

    [Range(0f, 1f)] public float HeroDefeatDuration = 0.35f;
}
