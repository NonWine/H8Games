// Narrow contract between "something happened in gameplay" and "a sound plays".
// Callers never touch AudioSource, so the mix can grow a mixer, ducking or a
// mute toggle without any listener changing.
public interface IAudioService
{
    void Play(SfxId id);

    // pitchMultiplier stacks on top of the entry's own random window. Used for
    // rising-pitch streaks, where each coin in a run sounds a step higher.
    void Play(SfxId id, float pitchMultiplier);

    void PlayMusic();

    void StopMusic();
}
