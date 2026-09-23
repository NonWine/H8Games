using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CaptureZoneController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image progressFillImage;
    [SerializeField] private CaptureZoneFeedbackView feedback;

    [Header("Capture")]
    [SerializeField, Min(0.01f)] private float captureDurationSeconds = 1.5f;

    [Header("Appearance")]
    [Tooltip("Wait before popping in, counted from the win. Give the territory zone time to " +
             "shrink into its circle first so the ring appears inside a settled shape.")]
    [SerializeField, Min(0f)] private float appearDelaySeconds = 0.6f;

    [SerializeField, Min(0.01f)] private float appearDurationSeconds = 0.45f;

    private SignalBus signalBus;
    private ITerritoryCaptureFocusProvider captureFocusProvider;

    private Vector3 rootBaseScale;
    private Tween appearTween;

    private float captureProgress;
    private bool isPlayerInside;
    private bool isCaptured;

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
        bool isCapturing = isPlayerInside && !isCaptured;

        if (isCapturing)
        {
            captureProgress = Mathf.Clamp01(captureProgress + Time.deltaTime / captureDurationSeconds);
            progressFillImage.fillAmount = captureProgress;

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
        if (isCaptured || other.GetComponentInParent<PlayerView>() == null)
            return;

        isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerView>() == null)
            return;

        isPlayerInside = false;
    }

    private void Show()
    {
        captureProgress = 0f;
        isPlayerInside = false;
        isCaptured = false;
        progressFillImage.fillAmount = 0f;

        if (feedback != null)
            feedback.ResetFeedback();

        MoveToCaptureFocus();

        appearTween?.Kill();
        root.transform.localScale = Vector3.zero;
        root.SetActive(true);

        appearTween = root.transform
            .DOScale(rootBaseScale, appearDurationSeconds)
            .SetDelay(appearDelaySeconds)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);
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
        root.transform.localScale = rootBaseScale;
        root.SetActive(false);

        if (feedback != null)
            feedback.PlayCompleted();

        signalBus.Fire<LevelCaptureCompletedSignal>();
    }
}
