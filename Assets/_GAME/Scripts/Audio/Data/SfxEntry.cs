using System;
using UnityEngine;

// One authored sound. Holds a clip list rather than a single clip because the
// noisiest sounds in this game (shots, hits, kills) fire many times per second,
// and a single repeated sample is the fastest way to make a mix sound cheap.
[Serializable]
public class SfxEntry
{
    public SfxId Id = SfxId.None;

    [Tooltip("Picked at random on every play. One clip is allowed; two or three " +
             "is what stops a burst of shots from sounding like a loop.")]
    public AudioClip[] Clips = Array.Empty<AudioClip>();

    [Range(0f, 1f)] public float Volume = 1f;

    [Tooltip("Random pitch window. A little variation per play does most of the " +
             "work that extra clips would.")]
    [Range(0.1f, 3f)] public float PitchMin = 0.95f;

    [Range(0.1f, 3f)] public float PitchMax = 1.05f;

    [Tooltip("Plays arriving sooner than this after the previous one of the same " +
             "id are dropped. Without it a squad volley fires a dozen identical " +
             "shots on the same frame and they sum into a click.")]
    [Min(0f)] public float MinIntervalSeconds = 0.04f;

    public bool HasClips => Clips != null && Clips.Length > 0;

    public AudioClip PickClip()
    {
        if (!HasClips)
        {
            return null;
        }

        return Clips.Length == 1 ? Clips[0] : Clips[UnityEngine.Random.Range(0, Clips.Length)];
    }

    public float PickPitch()
    {
        float min = Mathf.Min(PitchMin, PitchMax);
        float max = Mathf.Max(PitchMin, PitchMax);

        return UnityEngine.Random.Range(min, max);
    }
}
