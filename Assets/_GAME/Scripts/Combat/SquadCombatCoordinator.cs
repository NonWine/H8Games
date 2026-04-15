using UnityEngine;
using Zenject;

public class SquadCombatCoordinator : IAllyTargetProvider , ISoldierCombatRegistryProvider, ICurrentEnemyGroupProvider
{
    private readonly SquadSoldierRegistry soldierRegistry;
    private readonly SquadAllyTargetSelector allyTargetSelector;
    private readonly SquadEncounterController encounterController;
    private readonly SquadDefeatWatcher defeatWatcher;

    [Inject]
    public SquadCombatCoordinator(
        SquadSoldierRegistry soldierRegistry,
        SquadAllyTargetSelector allyTargetSelector,
        SquadEncounterController encounterController,
        SquadDefeatWatcher defeatWatcher)
    {
        this.soldierRegistry = soldierRegistry;
        this.allyTargetSelector = allyTargetSelector;
        this.encounterController = encounterController;
        this.defeatWatcher = defeatWatcher;
    }

    public EnemyGroupFacade CurrentTargetGroup => encounterController.CurrentTargetGroup;
    public bool HasActiveEncounter => encounterController.HasActiveEncounter;
    public bool HasLivingAllies => soldierRegistry.HasLivingAllies;

    public void RegisterSoldier(SoldierCombatAgent soldier)
    {
        soldierRegistry.Register(soldier);
        defeatWatcher.ResetForNewEncounter();
    }

    public void UnregisterSoldier(SoldierCombatAgent soldier)
    {
        soldierRegistry.Unregister(soldier);
        defeatWatcher.TryRaiseDefeat(HasActiveEncounter, HasLivingAllies);
    }

    public void StartBattle(EnemyGroupFacade enemyGroup)
    {
        encounterController.TryBeginEncounter(enemyGroup);
        defeatWatcher.ResetForNewEncounter();
        enemyGroup.Activate();
    }
 
    private void StartRegroupSoldiers()
    {
        
    }

    public ICombatTarget GetBestLivingAllyTarget(Vector3 worldPosition, float reservationPenalty)
    {
        return allyTargetSelector.GetBestLivingAllyTarget(worldPosition, reservationPenalty);
    }
}
