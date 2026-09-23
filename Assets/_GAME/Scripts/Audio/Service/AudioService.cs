using System.Collections.Generic;
using UnityEngine;
using Zenject;

// Decides what a given SfxId sounds like right now: which clip of the entry,
// at what pitch, and whether it is allowed to play at all. Holds no Unity audio
// type of its own - the pool does - so the rate limiting and clip choice can be
// tested without a scene.
public class AudioService : IAudioService, IInitializable
{
    private readonly AudioCatalog catalog;
    private readonly AudioSourcePool pool;

    private readonly Dictionary<SfxId, float> lastPlayTimeById = new Dictionary<SfxId, float>();

    public AudioService(AudioCatalog catalog, AudioSourcePool pool)
    {
        this.catalog = catalog;
        this.pool = pool;
    }

    public void Initialize()
    {
        pool.Prepare(catalog.VoiceCount);
        PlayMusic();
    }

    public void Play(SfxId id)
    {
        Play(id, 1f);
    }

    public void Play(SfxId id, float pitchMultiplier)
    {
        if (!catalog.TryGetEntry(id, out SfxEntry entry) || !entry.HasClips)
        {
            return;
        }

        // Unscaled, so a hit-stop cannot let a rate-limited sound through early
        // and turn one kill into a double hit.
        float now = Time.unscaledTime;
        if (lastPlayTimeById.TryGetValue(id, out float lastTime) &&
            now - lastTime < entry.MinIntervalSeconds)
        {
            return;
        }

        lastPlayTimeById[id] = now;

        pool.Prepare(catalog.VoiceCount);
        AudioSource source = pool.Next();
        source.clip = entry.PickClip();
        source.volume = entry.Volume * catalog.MasterSfxVolume;
        source.pitch = Mathf.Clamp(entry.PickPitch() * pitchMultiplier, 0.1f, 3f);
        source.Play();
    }

    public void PlayMusic()
    {
        if (catalog.MusicClip == null)
        {
            return;
        }

        AudioSource music = pool.Music;
        music.clip = catalog.MusicClip;
        music.volume = catalog.MusicVolume;
        music.Play();
    }

    public void StopMusic()
    {
        pool.Music.Stop();
    }
}
