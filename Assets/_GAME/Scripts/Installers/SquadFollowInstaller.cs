using UnityEngine;
using Zenject;

public class SquadFollowInstaller : MonoInstaller
{
    [SerializeField] private SquadFollowSettings settings;
    [SerializeField] private SquadRoot squadRoot;

    public override void InstallBindings()
    {
        if (settings == null)
            throw new System.InvalidOperationException("SquadFollowSettings is not assigned.");

        if (squadRoot == null)
            throw new System.InvalidOperationException("SquadRoot is not assigned.");

        Container.BindInstance(settings).AsSingle();
        Container.Bind<FormationLayoutService>().AsSingle();
        Container.Bind<SquadRoot>().FromInstance(squadRoot).AsSingle();
        Container.QueueForInject(squadRoot);
    }
}
