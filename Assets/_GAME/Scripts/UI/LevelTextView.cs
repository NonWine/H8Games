using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

public class LevelTextView : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private RectTransform targetTransform;

    [Header("Level Label")]
    [SerializeField] private string prefix = "LEVEL ";

    [Header("Color Fill")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.6f, 0f, 1f);

    [Header("Juice Timings & Motion")]
    [SerializeField, Min(0f)] private float bounceHeight = 25f;
    [SerializeField] private float tiltAngle = 7f;
    [SerializeField, Min(0.01f)] private float anticipationDuration = 0.06f;
    [SerializeField, Min(0.01f)] private float launchDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float fallDuration = 0.11f;
    [SerializeField, Min(0.01f)] private float impactDuration = 0.06f;
    [SerializeField, Min(0.01f)] private float settleDuration = 0.16f;

    [Header("Debug")]
    [SerializeField] private KeyCode debugKey = KeyCode.L;

    private LevelManager levelManager;
    private SignalBus signalBus;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPosition;
    private Vector3 startScale;
    private Quaternion startRotation;
    private Color startColor;
    private Sequence activeSequence;
    private int displayedLevel = 1;

    [Inject]
    public void Construct(LevelManager levelManager, SignalBus signalBus)
    {
        this.levelManager = levelManager;
        this.signalBus = signalBus;
    }

    private void Awake()
    {
        rectTransform = targetTransform != null ? targetTransform : (RectTransform)transform;
        startAnchoredPosition = rectTransform.anchoredPosition;
        startScale = rectTransform.localScale;
        startRotation = rectTransform.localRotation;
        startColor = levelText.color;
    }

    private void Start()
    {
        signalBus?.Subscribe<LevelTransitionCompletedSignal>(OnTransitionCompleted);
        signalBus?.Subscribe<LevelRestartedSignal>(OnLevelRestarted);

        SyncDisplayedLevelImmediate();
    }

    private void OnDestroy()
    {
        activeSequence?.Kill();
        signalBus?.TryUnsubscribe<LevelTransitionCompletedSignal>(OnTransitionCompleted);
        signalBus?.TryUnsubscribe<LevelRestartedSignal>(OnLevelRestarted);
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(debugKey))
        {
            PlayLevelUpAnimation(displayedLevel + 1);
        }
#endif
    }

    private void OnTransitionCompleted()
    {
        int target = levelManager != null ? levelManager.CurrentLevelIndex + 1 : displayedLevel + 1;
        PlayLevelUpAnimation(target);
    }

    private void OnLevelRestarted()
    {
        activeSequence?.Kill();
        ResetToStart();
        SyncDisplayedLevelImmediate();
    }

    public void PlayLevelUpAnimation(int newLevel)
    {
        activeSequence?.Kill();
        ResetToStart();

        RectTransform target = rectTransform;

        activeSequence = DOTween.Sequence()
            .SetLink(gameObject);

        activeSequence.Append(target.DOScale(new Vector3(1.15f, 0.85f, 1f), anticipationDuration).SetEase(Ease.InQuad));
        activeSequence.Join(target.DOAnchorPosY(startAnchoredPosition.y - 6f, anticipationDuration).SetEase(Ease.InQuad));

        activeSequence.Append(target.DOScale(new Vector3(0.85f, 1.25f, 1f), launchDuration).SetEase(Ease.OutQuad));
        activeSequence.Join(target.DOAnchorPosY(startAnchoredPosition.y + bounceHeight, launchDuration).SetEase(Ease.OutQuad));
        activeSequence.Join(target.DOLocalRotate(new Vector3(0f, 0f, -tiltAngle), launchDuration).SetEase(Ease.OutQuad));
        activeSequence.Join(levelText.DOColor(highlightColor, launchDuration * 0.75f).SetEase(Ease.OutQuad));

        activeSequence.AppendCallback(() =>
        {
            displayedLevel = newLevel;
            ApplyText(displayedLevel);
        });

        activeSequence.Append(target.DOAnchorPosY(startAnchoredPosition.y, fallDuration).SetEase(Ease.InQuad));
        activeSequence.Join(target.DOScale(new Vector3(0.95f, 1.05f, 1f), fallDuration).SetEase(Ease.InQuad));
        activeSequence.Join(target.DOLocalRotate(new Vector3(0f, 0f, tiltAngle * 0.7f), fallDuration).SetEase(Ease.InOutSine));

        activeSequence.Append(target.DOScale(new Vector3(1.22f, 0.82f, 1f), impactDuration).SetEase(Ease.OutQuad));
        activeSequence.Join(target.DOLocalRotate(new Vector3(0f, 0f, -tiltAngle * 0.25f), impactDuration).SetEase(Ease.OutQuad));

        activeSequence.Append(target.DOScale(startScale, settleDuration).SetEase(Ease.OutBack));
        activeSequence.Join(target.DOLocalRotateQuaternion(startRotation, settleDuration).SetEase(Ease.OutBack));
        activeSequence.Join(levelText.DOColor(startColor, settleDuration).SetEase(Ease.InOutSine));
    }

    private void ResetToStart()
    {
        rectTransform.anchoredPosition = startAnchoredPosition;
        rectTransform.localScale = startScale;
        rectTransform.localRotation = startRotation;
        levelText.color = startColor;
    }

    private void SyncDisplayedLevelImmediate()
    {
        displayedLevel = levelManager != null ? levelManager.CurrentLevelIndex + 1 : 1;
        ApplyText(displayedLevel);
    }

    private void ApplyText(int levelNumber)
    {
        levelText.text = prefix + levelNumber;
    }
}
