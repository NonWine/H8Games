using System;
using TMPro;
using UnityEngine;

public class FloatingNumberView : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private Action<FloatingNumberView> completed;
    private Vector3 startPosition;
    private Vector3 baseScale;
    private Color color;
    private float duration;
    private float riseDistance;
    private float emphasis;
    private float elapsed;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    public void Show(
        string value,
        Vector3 worldPosition,
        Color textColor,
        float fontSize,
        float animationDuration,
        float animationRiseDistance,
        float scaleEmphasis,
        Action<FloatingNumberView> onCompleted)
    {
        label.text = value;
        label.fontSize = fontSize;
        label.color = textColor;
        startPosition = worldPosition;
        transform.position = worldPosition;
        transform.localScale = Vector3.zero;
        color = textColor;
        duration = Mathf.Max(animationDuration, 0.01f);
        riseDistance = Mathf.Max(animationRiseDistance, 0f);
        emphasis = Mathf.Max(scaleEmphasis, 0f);
        elapsed = 0f;
        completed = onCompleted;
        FaceCamera();
    }

    private void Update()
    {
        elapsed = Mathf.Min(elapsed + Time.unscaledDeltaTime, duration);
        float progress = elapsed / duration;
        float riseProgress = 1f - Mathf.Pow(1f - progress, 3f);
        float fadeProgress = Mathf.InverseLerp(0.5f, 1f, progress);
        float popProgress = Mathf.Clamp01(progress / 0.16f);
        float overshoot = 1f + Mathf.Sin(popProgress * Mathf.PI) * 0.2f;

        transform.position = startPosition + Vector3.up * (riseDistance * riseProgress);
        transform.localScale = baseScale * (Mathf.SmoothStep(0f, emphasis, popProgress) * overshoot);
        label.color = new Color(color.r, color.g, color.b, 1f - fadeProgress);
        FaceCamera();

        if (elapsed >= duration)
        {
            Complete();
        }
    }

    private void FaceCamera()
    {
        Camera currentCamera = Camera.main;
        if (currentCamera != null)
        {
            transform.rotation = currentCamera.transform.rotation;
        }
    }

    private void Complete()
    {
        Action<FloatingNumberView> callback = completed;
        completed = null;
        callback?.Invoke(this);
    }

    private void OnDisable()
    {
        completed = null;
        transform.localScale = baseScale;
    }
}
