using UnityEngine;
using Zenject;

public class GameplayBeatInstaller : MonoInstaller
{
    [SerializeField] private GameplayBeatView view;
    [SerializeField] private GameplayBeatConfig config;

    public override void InstallBindings()
    {
        Container.BindInstance(view).AsSingle();
        Container.BindInstance(config).AsSingle();
        Container.BindInterfacesTo<GameplayBeatPresenter>().AsSingle().NonLazy();
    }
}
