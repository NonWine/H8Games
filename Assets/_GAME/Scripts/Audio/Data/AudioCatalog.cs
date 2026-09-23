using System.Collections.Generic;
using UnityEngine;

// Single authored source of truth for what every gameplay event sounds like.
// Pure data: the mapping from a signal to an SfxId lives in the listener that
// owns that signal, the same split the camera shake feature already uses.
[CreateAssetMenu(fileName = "AudioCatalog", menuName = "Config/AudioCatalog")]
public class AudioCatalog : ScriptableObject
{
    [Header("Global")]
    [Range(0f, 1f)] public float MasterSfxVolume = 1f;

    [Tooltip("How many one-shots can overlap. Past this the oldest source is " +
             "reused, which is what keeps a wipe from blowing out the mix.")]
    [Range(4, 32)] public int VoiceCount = 16;

    [Header("Music")]
    public AudioClip MusicClip;

    [Range(0f, 1f)] public float MusicVolume = 0.35f;

    [Header("Coin streak")]
    [Tooltip("Each coin in an unbroken run plays a step higher than the last. " +
             "The single cheapest piece of juice in a collector game.")]
    [Min(0f)] public float CoinStreakPitchStep = 0.045f;

    [Min(1f)] public float CoinStreakMaxPitch = 1.8f;

    [Tooltip("A gap this long ends the run and drops the pitch back to base.")]
    [Min(0.05f)] public float CoinStreakResetSeconds = 1.2f;

    [Header("Sounds")]
    public List<SfxEntry> Entries = new List<SfxEntry>();

    private Dictionary<SfxId, SfxEntry> entriesById;

    public bool TryGetEntry(SfxId id, out SfxEntry entry)
    {
        BuildLookupIfNeeded();

        return entriesById.TryGetValue(id, out entry);
    }

    // Rebuilt on demand rather than in OnEnable: a domain reload or an edit to
    // the list in the inspector during play must not leave a stale map behind.
    private void BuildLookupIfNeeded()
    {
        if (entriesById != null && entriesById.Count == Entries.Count)
        {
            return;
        }

        entriesById = new Dictionary<SfxId, SfxEntry>(Entries.Count);

        for (int i = 0; i < Entries.Count; i++)
        {
            SfxEntry entry = Entries[i];
            if (entry == null || entry.Id == SfxId.None)
            {
                continue;
            }

            entriesById[entry.Id] = entry;
        }
    }
}
