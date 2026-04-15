using System;
using Zenject;

public class SquadCombatPresenter : IInitializable, IDisposable
{
    private readonly SquadCombatCoordinator squadCombatCoordinator;
    private readonly SquadEncounterController squadEncounterController;
    private readonly SquadDefeatWatcher squadDefeatWatcher;
    private readonly CombatStateController combatStateController;

    public SquadCombatPresenter(
        SquadCombatCoordinator squadCombatCoordinator,
        SquadEncounterController squadEncounterController,
        SquadDefeatWatcher squadDefeatWatcher,
        CombatStateController combatStateController)
    {
        this.squadCombatCoordinator = squadCombatCoordinator;
        this.squadEncounterController = squadEncounterController;
        this.squadDefeatWatcher = squadDefeatWatcher;
        this.combatStateController = combatStateController;
    }

    public void Initialize()
    {
        squadEncounterController.CombatStartedBattle += combatStateController.HandleCombatStartedBattle;
        squadEncounterController.CombatClearedZone += combatStateController.HandleCombatClearedZone;
        squadDefeatWatcher.SquadDefeated += combatStateController.SetDefeated;
    }

    public void Dispose()
    {
        squadEncounterController.CombatStartedBattle -= combatStateController.HandleCombatStartedBattle;
        squadEncounterController.CombatClearedZone -= combatStateController.HandleCombatClearedZone;
        squadDefeatWatcher.SquadDefeated -= combatStateController.SetDefeated;
    }

    public void NotifyEncounterZoneEntered(EnemyGroupFacade enemyGroup)
    {
        if (combatStateController.CurrentTargetGroup != enemyGroup)
            return;

        squadCombatCoordinator.StartBattle(enemyGroup);
    }
}