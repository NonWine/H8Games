using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameplayBeatView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform content;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text label;

    private Sequence sequence;
    private Vector2 basePosition;
    private Vector3 baseScale;

    private void Awake()
    {
        basePosition = content.anchoredPosition;
        baseScale = content.localScale;
        HideImmediate();
    }

    public void Show(string text, Color color, float holdDuration, GameplayBeatConfig config)
    {
        sequence?.Kill();

        float enterDuration = Mathf.Max(config.EnterDuration, 0.01f);
        float settleDuration = Mathf.Max(config.SettleDuration, 0.01f);
        float exitDuration = Mathf.Max(config.ExitDuration, 0.01f);

        root.SetActive(true);
        label.text = text;
        label.color = color;
        canvasGroup.alpha = 0f;
        content.anchoredPosition = basePosition - Vector2.up * config.EnterOffset;
        content.localScale = baseScale * config.EnterScale;

        sequence = DOTween.Sequence()
            .SetUpdate(true)
            .SetLink(gameObject)
            .Append(content.DOAnchorPos(basePosition, enterDuration).SetEase(Ease.OutCubic))
            .Join(content.DOScale(baseScale * config.OvershootScale, enterDuration).SetEase(Ease.OutBack))
            .Join(canvasGroup.DOFade(1f, enterDuration * 0.75f))
            .Append(content.DOScale(baseScale, settleDuration).SetEase(Ease.OutCubic))
            .AppendInterval(Mathf.Max(holdDuration, 0f))
            .Append(content.DOAnchorPos(basePosition + Vector2.up * config.EnterOffset * 0.5f, exitDuration).SetEase(Ease.InCubic))
            .Join(canvasGroup.DOFade(0f, exitDuration))
            .OnComplete(HideImmediate);
    }

    private void HideImmediate()
    {
        canvasGroup.alpha = 0f;
        content.anchoredPosition = basePosition;
        content.localScale = baseScale;
        root.SetActive(false);
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }
}
