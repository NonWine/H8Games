using UnityEngine;
using Zenject;

public class CombatFlowController : MonoBehaviour
{
    [SerializeField] private PlayerView playerView;
    [SerializeField] private BarracksFacade enemyBarracks;
    [SerializeField] private WinLoseView winLoseView;

    private CombatEndService combatEndService;

    [Inject]
    public void Construct(CombatEndService combatEndService, [InjectOptional] PlayerView injectedHero = null)
    {
        this.combatEndService = combatEndService;
        this.combatEndService.Finished += HandleFinished;

        if (playerView == null)
            playerView = injectedHero;
    }

    private void OnEnable()
    {
        if (playerView != null)
            playerView.Died += HandleHeroDied;

        if (enemyBarracks != null)
            enemyBarracks.Destroyed += HandleEnemyBarracksDestroyed;
    }

    private void OnDisable()
    {
        if (playerView != null)
            playerView.Died -= HandleHeroDied;

        if (enemyBarracks != null)
            enemyBarracks.Destroyed -= HandleEnemyBarracksDestroyed;
    }

    private void OnDestroy()
    {
        if (combatEndService != null)
            combatEndService.Finished -= HandleFinished;
    }

    private void HandleHeroDied()
    {
        combatEndService?.FinishLose();
    }

    private void HandleEnemyBarracksDestroyed(BarracksFacade barracks)
    {
        combatEndService?.FinishWin();
    }

    private void HandleFinished(CombatResult result)
    {
        winLoseView?.Show(result);
    }
}
