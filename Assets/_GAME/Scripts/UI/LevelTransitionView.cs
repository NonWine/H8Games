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

    [Header("Timings")]
    [Tooltip("Wait before fading to allow the 'CAPTURED' text to play.")]
    [SerializeField, Min(0f)] private float startDelay = 0.75f;
    [SerializeField, Min(0f)] private float zoomInDuration = 0.25f;
    [SerializeField, Min(0f)] private float zoomOutDuration = 0.35f;
    
    private void Awake()
    {
        zoomImage.rectTransform.localScale = Vector3.one;
        SetAlpha(0f);
        zoomImage.raycastTarget = false;
        
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
        zoomImage.raycastTarget = true;

        if (startDelay > 0f)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(startDelay), ignoreTimeScale: false);
        }

        await FadeAsync(1f, zoomInDuration);

        signalBus.Fire(new LoadNextLevelSignal());
        TeleportHeroToLevelStart();
        levelManager.CurrentLevel?.ResetRuntimeState();

        await FadeAsync(0f, zoomOutDuration);
        
        zoomImage.raycastTarget = false;
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

    private UniTask FadeAsync(float targetAlpha, float duration)
    {
        zoomTween?.Kill();

        UniTaskCompletionSource completionSource = new UniTaskCompletionSource();
        zoomTween = zoomImage.DOFade(targetAlpha, duration)
            .SetEase(Ease.Linear)
            .SetLink(gameObject)
            .OnComplete(() => completionSource.TrySetResult());

        return completionSource.Task;
    }
    
    private void SetAlpha(float alpha)
    {
        Color c = zoomImage.color;
        c.a = alpha;
        zoomImage.color = c;
    }
}
