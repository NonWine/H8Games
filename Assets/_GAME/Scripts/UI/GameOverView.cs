using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameOverView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button restartButton;
    [SerializeField] private Transform spawnPoint;

    [Inject] private SignalBus signalBus;
    [Inject] private SquadCombatStateController squadCombatStateController;
    [Inject] private HeroCombatAgentController heroCombatAgentController;

    private bool heroDiedSinceLastIdle;

    private void Awake()
    {
        signalBus.Subscribe<HeroDefeatedSignal>(HandleHeroDefeated);
        signalBus.Subscribe<GameIdleStateSignal>(HandleGameIdle);
        signalBus.Subscribe<StartButtleSignal>(Hide);
        restartButton.onClick.AddListener(HandleRestartClicked);
    }

    private void OnDestroy()
    {
        signalBus.Unsubscribe<HeroDefeatedSignal>(HandleHeroDefeated);
        signalBus.Unsubscribe<GameIdleStateSignal>(HandleGameIdle);
        signalBus.Unsubscribe<StartButtleSignal>(Hide);
        restartButton.onClick.RemoveListener(HandleRestartClicked);
    }

    private void HandleRestartClicked()
    {
        heroCombatAgentController.RestartAtSpawn(spawnPoint.position, spawnPoint.rotation);
        squadCombatStateController.RestartLevel();
        Hide();
    }

    private void HandleHeroDefeated()
    {
        heroDiedSinceLastIdle = true;
    }

    private void HandleGameIdle()
    {
        if (heroDiedSinceLastIdle)
        {
            Show();
        }

        heroDiedSinceLastIdle = false;
    }

    private void Show()
    {
        root.SetActive(true);
    }

    private void Hide()
    {
        root.SetActive(false);
    }
}
