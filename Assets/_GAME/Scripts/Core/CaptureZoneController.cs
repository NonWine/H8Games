using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CaptureZoneController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject root;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private CaptureZoneFeedbackView feedback;

    [Header("Capture")]
    [SerializeField, Min(0.01f)] private float captureDurationSeconds = 1.5f;

    [Header("Appearance")]
    [Tooltip("Wait before popping in, counted from the win. Give the territory zone time to " +
             "shrink into its circle first so the ring appears inside a settled shape.")]
    [SerializeField, Min(0f)] private float appearDelaySeconds = 0.6f;

    [SerializeField, Min(0.01f)] private float appearDurationSeconds = 0.45f;

    [Header("Preview")]
    [SerializeField, Min(0f)] private float previewRestartDelaySeconds = 0.6f;

    private SignalBus signalBus;
    private ITerritoryCaptureFocusProvider captureFocusProvider;

    private Vector3 rootBaseScale;
    private Tween appearTween;

    private float captureProgress;
    private float previewRestartTimer;
    private bool isPlayerInside;
    private bool isCaptured;
    private bool previewMode;

    [Inject]
    public void Construct(
        SignalBus signalBus,
        [InjectOptional] ITerritoryCaptureFocusProvider captureFocusProvider)
    {
        this.signalBus = signalBus;
        this.captureFocusProvider = captureFocusProvider;
    }

    private void Awake()
    {
        // Read before the first hide: the pop-in scales from zero back to this.
        rootBaseScale = root.transform.localScale;

        // The controller works in 0..1, so the fill never depends on whatever
        // range the slider happens to be authored with.
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;

        root.SetActive(false);
        signalBus.Subscribe<LevelCompletedSignal>(Show);
    }

    private void OnDestroy()
    {
        appearTween?.Kill();
        signalBus.Unsubscribe<LevelCompletedSignal>(Show);
    }

    private void Update()
    {
        if (previewMode && isCaptured)
        {
            previewRestartTimer -= Time.deltaTime;

            if (previewRestartTimer <= 0f)
                RestartPreviewLoop();

            return;
        }

        bool isCapturing = isPlayerInside && !isCaptured;

        if (isCapturing)
        {
            captureProgress = Mathf.Clamp01(captureProgress + Time.deltaTime / captureDurationSeconds);
            progressSlider.value = captureProgress;

            if (captureProgress >= 1f)
            {
                Complete();
                return;
            }
        }

        if (feedback != null)
            feedback.Tick(captureProgress, isCapturing && root.activeSelf, Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (previewMode || isCaptured || other.GetComponentInParent<PlayerView>() == null)
            return;

        isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (previewMode || other.GetComponentInParent<PlayerView>() == null)
            return;

        isPlayerInside = false;
    }

    public void SetPreviewMode(bool enabled)
    {
        if (previewMode == enabled)
            return;

        previewMode = enabled;

        if (enabled)
        {
            RestartPreviewLoop();
            return;
        }

        appearTween?.Kill();
        root.SetActive(false);
        isPlayerInside = false;
        isCaptured = false;
        captureProgress = 0f;
    }

    private void Show()
    {
        captureProgress = 0f;
        isPlayerInside = false;
        isCaptured = false;
        progressSlider.value = 0f;

        if (feedback != null)
            feedback.ResetFeedback();

        if (!previewMode)
            MoveToCaptureFocus();

        appearTween?.Kill();
        root.transform.localScale = Vector3.zero;
        root.SetActive(true);

        appearTween = root.transform
            .DOScale(rootBaseScale, appearDurationSeconds)
            .SetDelay(appearDelaySeconds)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);

        if (previewMode)
            appearTween.OnComplete(BeginPreviewCapture);
    }

    private void BeginPreviewCapture()
    {
        isPlayerInside = true;
    }

    // Only the ground plane moves. The zone is a flat world-space canvas and its
    // authored height is what keeps it from z-fighting the territory mesh, so the
    // Y the level was built with is preserved.
    private void MoveToCaptureFocus()
    {
        if (captureFocusProvider == null || !captureFocusProvider.TryGetCaptureFocus(out Vector3 focus))
            return;

        Vector3 position = transform.position;
        transform.position = new Vector3(focus.x, position.y, focus.z);
    }

    private void Complete()
    {
        isCaptured = true;

        appearTween?.Kill();

        if (feedback != null)
            feedback.PlayCompleted();

        if (previewMode)
        {
            appearTween = root.transform
                .DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .SetLink(gameObject);
                
            previewRestartTimer = previewRestartDelaySeconds;
            return;
        }

        signalBus.Fire<LevelCaptureCompletedSignal>();

        appearTween = root.transform
            .DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .SetLink(gameObject)
            .OnComplete(() => root.SetActive(false));
    }

    private void RestartPreviewLoop()
    {
        Show();
    }
}
