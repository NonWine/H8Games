using UnityEngine;
using UnityEngine.UI;
using Zenject;

// Lives on an always-active node so its signal subscriptions survive scene
// load; the actual button graphic is a separate child toggled by Show/Hide,
// same split StartButtonInteractorView uses and for the same reason - Awake
// never runs on an object that starts inactive.
//
// SetDefeated() runs the same recovery for both HeroDefeatedSignal and
// SquadDefeatedSignal, so GameIdleStateSignal alone can't tell which one
// happened - a soldiers-only wipe reaches the exact same idle state while the
// hero is still alive and fighting. Restart only makes sense for the player's
// own death, so HeroDefeatedSignal is latched separately and only shown once
// GameIdleStateSignal confirms the squad actually reached the idle window
// (showing earlier would sit through the 2s recovery delay where a click is
// still a no-op against SquadCombatStateController's own state guard).
public class RestartButtonView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Button restartButton;

    [Inject] private SignalBus signalBus;
    [Inject] private SquadCombatStateController squadCombatStateController;

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
