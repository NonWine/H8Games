using System;
using UnityEngine;
using Zenject;

public class SquadCombatInstaller : MonoInstaller
{
    [SerializeField] private SquadCombatCoordinator squadCombatCoordinator;
    [SerializeField] private SquadFlowCoordinator squadFlowCoordinator;
    [SerializeField] private LevelRuntime[] levels;
    [SerializeField] private int startingLevelIndex;
    [SerializeField] private GamePhase initialPhase = GamePhase.Preparation;

    public override void InstallBindings()
    {
        if (squadCombatCoordinator == null)
            throw new InvalidOperationException("SquadCombatCoordinator is not assigned.");

        if (squadFlowCoordinator == null)
            throw new InvalidOperationException("SquadFlowCoordinator is not assigned.");

        CampaignService campaignService = new CampaignService(levels, startingLevelIndex);
        LevelManager levelManager = new LevelManager(campaignService);
        GamePhaseService gamePhaseService = new GamePhaseService(initialPhase);

        Container.Bind<SquadCombatCoordinator>().FromInstance(squadCombatCoordinator).AsSingle();
        Container.QueueForInject(squadCombatCoordinator);
        Container.Bind<SquadFlowCoordinator>().FromInstance(squadFlowCoordinator).AsSingle();
        Container.QueueForInject(squadFlowCoordinator);
        Container.BindInstance(campaignService).AsSingle();
        Container.BindInstance(levelManager).AsSingle();
        Container.BindInstance(gamePhaseService).AsSingle();
    }
}
