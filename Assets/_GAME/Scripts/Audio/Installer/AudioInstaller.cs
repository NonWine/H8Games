using UnityEngine;
using Zenject;

public class AudioInstaller : MonoInstaller
{
    [SerializeField] private AudioCatalog catalog;
    [SerializeField] private AudioSourcePool sourcePool;

    public override void InstallBindings()
    {
        Container.Bind<AudioCatalog>().FromInstance(catalog).AsSingle();
        Container.Bind<AudioSourcePool>().FromInstance(sourcePool).AsSingle();
        Container.BindInterfacesTo<AudioService>().AsSingle();
        Container.BindInterfacesTo<GameplayAudioListener>().AsSingle().NonLazy();
        Container.BindInterfacesTo<CombatAudioListener>().AsSingle().NonLazy();
    }
}
