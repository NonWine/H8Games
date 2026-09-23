using System;
using Zenject;

// Combat's own sounds, kept apart from GameplayAudioListener because they are
// driven by per-unit events firing dozens of times a second rather than by the
// handful of flow signals a level emits.
//
// The shot itself is not here: it is played where the projectile is spawned, so
// it lands on the frame the muzzle flash does instead of one signal hop later.
public class CombatAudioListener : IInitializable, IDisposable
{
    private readonly SignalBus signalBus;
    private readonly IAudioService audio;

    public CombatAudioListener(SignalBus signalBus, IAudioService audio)
    {
        this.signalBus = signalBus;
        this.audio = audio;
    }

    public void Initialize()
    {
        signalBus.Subscribe<UnitDamagedSignal>(OnUnitDamaged);
        signalBus.Subscribe<UnitDiedSignal>(OnUnitDied);
    }

    public void Dispose()
    {
        signalBus.Unsubscribe<UnitDamagedSignal>(OnUnitDamaged);
        signalBus.Unsubscribe<UnitDiedSignal>(OnUnitDied);
    }

    // The lethal hit is skipped: the kill sound fires a frame later for the same
    // event, and playing both turns every kill into a double tap.
    private void OnUnitDamaged(UnitDamagedSignal signal)
    {
        if (signal.WasLethal)
        {
            return;
        }

        audio.Play(SfxId.Hit);
    }

    private void OnUnitDied(UnitDiedSignal signal)
    {
        audio.Play(SfxId.Kill);
    }
}
