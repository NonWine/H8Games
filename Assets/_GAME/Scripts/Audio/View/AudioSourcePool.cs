using UnityEngine;

// The only Unity-audio-aware piece of the feature: owns the AudioSource objects
// so AudioService stays a plain class that decides what to play and when.
//
// Every source is built at runtime rather than serialized, so the scene side of
// this feature is one empty GameObject with this component and nothing to wire
// up or to get wrong.
[DisallowMultipleComponent]
public class AudioSourcePool : MonoBehaviour
{
    private AudioSource[] voices;
    private AudioSource musicSource;
    private int nextVoiceIndex;

    public AudioSource Music => musicSource;

    public void Prepare(int voiceCount)
    {
        if (voices != null)
        {
            return;
        }

        musicSource = CreateSource("Music");
        musicSource.loop = true;

        voices = new AudioSource[Mathf.Max(1, voiceCount)];
        for (int i = 0; i < voices.Length; i++)
        {
            voices[i] = CreateSource($"Voice_{i:00}");
        }
    }

    // Round-robin rather than "first free": looking for a free source means a
    // sustained fight silently drops its newest sounds once every voice is busy,
    // and the newest sound is the one the player just caused.
    public AudioSource Next()
    {
        AudioSource source = voices[nextVoiceIndex];

        nextVoiceIndex++;
        if (nextVoiceIndex == voices.Length)
        {
            nextVoiceIndex = 0;
        }

        return source;
    }

    // spatialBlend stays at 0 (2D). The camera is a fixed top-down follow, so
    // everything that matters is on screen anyway, and 2D removes a whole class
    // of "why is this inaudible" falloff tuning from a one-minute slice.
    private AudioSource CreateSource(string sourceName)
    {
        GameObject host = new GameObject(sourceName);
        host.transform.SetParent(transform, false);

        AudioSource source = host.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        return source;
    }
}
