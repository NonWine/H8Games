using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LevelTransitionView : MonoBehaviour
{
    [SerializeField] private Image zoomImage;

    [Inject] private SignalBus signalBus;
    [Inject] private LevelManager levelManager;
    [Inject] private HeroCombatAgentController heroCombatAgentController;

    private Tween zoomTween;

    [SerializeField, Min(0f)] private float zoomInDuration = 0.25f;
    [SerializeField, Min(0f)] private float zoomOutDuration = 0.35f;
    
    private void Awake()
    {
        zoomImage.rectTransform.localScale = Vector3.zero;
        signalBus.Subscribe<LevelCaptureCompletedSignal>(HandleLevelCaptured);
    }

    private void OnDestroy()
    {
        signalBus.Unsubscribe<LevelCaptureCompletedSignal>(HandleLevelCaptured);
        zoomTween?.Kill();
    }

    private void HandleLevelCaptured()
    {
        RunTransitionAsync().Forget();
    }
    
    private async UniTaskVoid RunTransitionAsync()
    {
        await ZoomAsync(Vector3.one, zoomInDuration);

        signalBus.Fire(new LoadNextLevelSignal());
        TeleportHeroToLevelStart();
        levelManager.CurrentLevel?.ResetRuntimeState();

        await ZoomAsync(Vector3.zero, zoomOutDuration);
    }

    // Runs while the zoom overlay is still fully covering the screen, right
    // before enemies respawn, so the hero is already at the new encounter's
    // start point before any enemy can wake up and see him there.
    private void TeleportHeroToLevelStart()
    {
        Transform startPoint = levelManager.CurrentLevel?.StartPoint;
        if (startPoint == null)
        {
            Debug.LogError("LevelTransitionView: current level has no StartPoint assigned - hero will not be repositioned before enemies respawn.", this);
            return;
        }

        heroCombatAgentController.TeleportTo(startPoint.position, startPoint.rotation);
    }

    private UniTask ZoomAsync(Vector3 targetScale, float duration)
    {
        zoomTween?.Kill();

        UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
        zoomTween = zoomImage.rectTransform
            .DOScale(targetScale, duration)
            .SetLink(gameObject)
            .OnComplete(() => completionSource.TrySetResult());

        return completionSource.Task;
    }
}
