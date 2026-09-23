using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingTextView : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;

    private Sequence sequence;
    private Action<FloatingTextView> released;

    public void Prepare(Action<FloatingTextView> releaseCallback)
    {
        released = releaseCallback;
        gameObject.SetActive(false);
    }

    public void Play(string text, Color color, Vector3 worldPosition, Quaternion facing, FloatingTextConfig config)
    {
        sequence?.Kill();

        label.text = text;
        label.color = color;

        transform.SetPositionAndRotation(worldPosition, facing);
        transform.localScale = Vector3.one * config.StartScale;
        gameObject.SetActive(true);

        float peakDuration = config.Duration * config.PeakScaleTime;
        float fadeDelay = config.Duration * config.FadeStartTime;

        label.alpha = 1f;

        sequence = DOTween.Sequence()
            .Append(transform.DOMoveY(worldPosition.y + config.RiseDistance, config.Duration).SetEase(config.RiseEase))
            .Join(transform.DOScale(config.PeakScale, peakDuration).SetEase(Ease.OutBack))
            .Join(DOTween.To(GetAlpha, SetAlpha, 0f, config.Duration - fadeDelay).SetDelay(fadeDelay))
            .SetLink(gameObject)
            .OnComplete(Release);
    }

    public void Release()
    {
        sequence?.Kill();
        sequence = null;

        gameObject.SetActive(false);
        released?.Invoke(this);
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    private float GetAlpha() => label.alpha;

    private void SetAlpha(float value) => label.alpha = value;
}
