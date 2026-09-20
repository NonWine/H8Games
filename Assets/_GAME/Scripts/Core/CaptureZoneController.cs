using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CaptureZoneController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image progressFillImage;

    [Header("Capture")]
    [SerializeField, Min(0.01f)] private float captureDurationSeconds = 1.5f;

    private SignalBus signalBus;

    private float captureProgress;
    private bool isPlayerInside;
    private bool isCaptured;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        this.signalBus = signalBus;
    }

    private void Awake()
    {
        root.SetActive(false);
        signalBus.Subscribe<LevelCompletedSignal>(Show);
    }

    private void OnDestroy()
    {
        signalBus.Unsubscribe<LevelCompletedSignal>(Show);
    }

    private void Update()
    {
        if (!isPlayerInside || isCaptured)
            return;

        captureProgress = Mathf.Clamp01(captureProgress + Time.deltaTime / captureDurationSeconds);
        progressFillImage.fillAmount = captureProgress;

        if (captureProgress >= 1f)
            Complete();
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
        root.SetActive(true);
    }
    
    private void Complete()
    {
        isCaptured = true;
        root.SetActive(false);
        signalBus.Fire<LevelCaptureCompletedSignal>();
    }
}
