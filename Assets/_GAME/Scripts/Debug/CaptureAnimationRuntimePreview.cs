using UnityEngine;

public class CaptureAnimationRuntimePreview : MonoBehaviour
{
    [SerializeField] private CaptureZoneController captureZone;
    [SerializeField] private bool forceCapturePreview;

    private bool appliedValue;
    private bool isApplied;

    private void Start()
    {
        Apply();
    }

    private void Update()
    {
        if (!isApplied || appliedValue != forceCapturePreview)
        {
            Apply();
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && captureZone != null)
        {
            Apply();
        }
    }

    private void OnDisable()
    {
        if (captureZone != null)
        {
            captureZone.SetPreviewMode(false);
        }

        isApplied = false;
    }

    private void Apply()
    {
        if (captureZone == null)
        {
            return;
        }

        appliedValue = forceCapturePreview;
        isApplied = true;
        captureZone.SetPreviewMode(forceCapturePreview);
    }
}
