using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LevelTransitionView : MonoBehaviour
{
    [SerializeField] private Image zoomImage;

    [Header("Timings")]
    [SerializeField, Min(0f)] private float startDelay = 0.5f;
    [SerializeField, Min(0.01f)] private float zoomInDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float zoomOutDuration = 0.35f;

    private SignalBus signalBus;
    private LevelManager levelManager;
    private HeroCombatAgentController heroCombatAgentController;
    private Tween activeTween;

    [Inject]
    public void Construct(
        SignalBus signalBus,
        LevelManager levelManager,
        HeroCombatAgentController heroCombatAgentController)
    {
        this.signalBus = signalBus;
        this.levelManager = levelManager;
        this.heroCombatAgentController = heroCombatAgentController;
    }

    private void Awake()
    {
        if (zoomImage != null)
        {
            zoomImage.rectTransform.localScale = Vector3.zero;
            SetAlpha(0f);
            zoomImage.raycastTarget = false;
        }
    }

    private void Start()
    {
        signalBus?.Subscribe<LevelCaptureCompletedSignal>(HandleLevelCaptured);
    }

    private void OnDestroy()
    {
        signalBus?.TryUnsubscribe<LevelCaptureCompletedSignal>(HandleLevelCaptured);
        activeTween?.Kill();
    }

    private void HandleLevelCaptured()
    {
        RunTransitionAsync().Forget();
    }

    private async UniTaskVoid RunTransitionAsync()
    {
        if (zoomImage == null)
        {
            return;
        }

        zoomImage.raycastTarget = true;

        if (startDelay > 0f)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(startDelay), ignoreTimeScale: false);
        }

        await PlayTransitionInAsync();

        signalBus.Fire(new LoadNextLevelSignal());
        TeleportHeroToLevelStart();
        levelManager.CurrentLevel?.ResetRuntimeState();

        await PlayTransitionOutAsync();

        zoomImage.raycastTarget = false;
        signalBus.Fire<LevelTransitionCompletedSignal>();
    }

    private void TeleportHeroToLevelStart()
    {
        Transform startPoint = levelManager.CurrentLevel?.StartPoint;
        if (startPoint == null)
        {
            return;
        }

        heroCombatAgentController.TeleportTo(startPoint.position, startPoint.rotation);
    }

    private UniTask PlayTransitionInAsync()
    {
        activeTween?.Kill();
        var tcs = new UniTaskCompletionSource();

        Sequence seq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(zoomImage.rectTransform.DOScale(Vector3.one, zoomInDuration).SetEase(Ease.OutCubic))
            .Join(zoomImage.DOFade(1f, zoomInDuration * 0.8f).SetEase(Ease.OutQuad))
            .OnComplete(() => tcs.TrySetResult());

        activeTween = seq;
        return tcs.Task;
    }

    private UniTask PlayTransitionOutAsync()
    {
        activeTween?.Kill();
        var tcs = new UniTaskCompletionSource();

        Sequence seq = DOTween.Sequence()
            .SetLink(gameObject)
            .Append(zoomImage.rectTransform.DOScale(Vector3.zero, zoomOutDuration).SetEase(Ease.InCubic))
            .Join(zoomImage.DOFade(0f, zoomOutDuration).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                SetAlpha(0f);
                tcs.TrySetResult();
            });

        activeTween = seq;
        return tcs.Task;
    }

    private void SetAlpha(float alpha)
    {
        if (zoomImage == null) return;
        Color c = zoomImage.color;
        c.a = alpha;
        zoomImage.color = c;
    }
}
