using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BarracksUpgradeZoneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI priceLabel;

    [Header("Upgrade")]
    [SerializeField] private SquadBarracksSpawner barracksSpawner;
    [SerializeField, Min(1)] private int requiredCoins = 99;

    [Header("Sequential Toss")]
    [SerializeField] private string pickupId = "coin";
    [SerializeField] private Transform coinThrowTarget;
    [SerializeField, Min(0.01f)] private float tossInterval = 0.08f;
    [SerializeField, Min(0.01f)] private float minTossInterval = 0.03f;
    [SerializeField, Min(0f)] private float acceleratePerToss = 0.004f;
    [SerializeField, Min(1)] private int maxConcurrentInFlight = 6;

    [Header("Panel Animation")]
    [SerializeField, Min(0f)] private float showDuration = 0.18f;
    [SerializeField, Min(0f)] private float fillDuration = 0.2f;
    [SerializeField, Min(0f)] private float hideDuration = 0.2f;

    [Header("Juice")]
    [SerializeField] private ParticleSystem arrivalBurst;
    [SerializeField] private AudioSource tossAudioSource;
    [SerializeField] private AudioClip tossClip;
    [SerializeField, Min(0.1f)] private float baseTossPitch = 1f;
    [SerializeField, Min(0f)] private float tossPitchStep = 0.03f;
    [SerializeField, Min(0.1f)] private float maxTossPitch = 1.6f;
    [SerializeField, Min(0f)] private float labelPunch = 0.18f;
    [SerializeField] private Transform completionShakeTarget;
    [SerializeField, Min(0f)] private float completionShakeStrength = 0.4f;
    [SerializeField, Min(0f)] private float completionShakeDuration = 0.3f;

    private CurrencyService currencyService;
    private IPickupService pickupService;
    private IPickupCarryAnchorProvider carryAnchorProvider;
    private Tween fillTween;
    private Tween panelTween;

    private bool isPlayerInside;
    private bool isCompleted;
    private int spentCoins;
    private int inFlightCount;
    private int tossStreakForPitch;
    private float currentInterval;
    private float tossTimer;

    [Inject]
    public void Construct(CurrencyService currencyService, IPickupService pickupService)
    {
        this.currencyService = currencyService;
        this.pickupService = pickupService;
    }

    private void Awake()
    {
        currentInterval = tossInterval;
        RefreshVisualState(animate: false);
    }

    private void OnDestroy()
    {
        fillTween?.Kill();
        panelTween?.Kill();
    }

    private void Update()
    {
        if (!isPlayerInside || isCompleted)
            return;

        if (!CanLaunchToss())
            return;

        tossTimer += Time.deltaTime;

        if (tossTimer < currentInterval)
            return;

        tossTimer -= currentInterval;
        LaunchToss();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerView player = other.GetComponentInParent<PlayerView>();

        if (player == null)
            return;

        BeginSession(player);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerView>() == null)
            return;

        EndSession();
    }

    private void BeginSession(PlayerView player)
    {
        if (isCompleted || isPlayerInside)
            return;

        carryAnchorProvider = player;
        isPlayerInside = true;
        currentInterval = tossInterval;
        tossTimer = 0f;
        tossStreakForPitch = 0;

        panelRoot.transform.DOScale(1.2f, showDuration).SetEase(Ease.OutBack);
    }

    private void EndSession()
    {
        if (!isPlayerInside)
            return;

        isPlayerInside = false;
        carryAnchorProvider = null;

        if (!isCompleted)
            panelRoot.transform.DOScale(1f, showDuration).SetEase(Ease.Linear);
    }

    private bool CanLaunchToss()
    {
        int remaining = requiredCoins - spentCoins - inFlightCount;
        int affordable = currencyService.Amount - inFlightCount;

        return remaining > 0 && affordable > 0 && inFlightCount < maxConcurrentInFlight;
    }

    private void LaunchToss()
    {
        Vector3 origin = carryAnchorProvider != null && carryAnchorProvider.TryGetAnchor(out Transform anchor)
            ? anchor.position
            : transform.position;

        Transform target = coinThrowTarget != null ? coinThrowTarget : transform;

        inFlightCount++;
        pickupService.TossDeposit(pickupId, origin, target, OnUnitArrived);

        PlayTossSfx();
        currentInterval = Mathf.Max(minTossInterval, currentInterval - acceleratePerToss);
    }

    private void OnUnitArrived()
    {
        inFlightCount = Mathf.Max(0, inFlightCount - 1);

        if (isCompleted)
            return;

        if (!currencyService.TrySpend(1))
        {
            RefreshVisualState(animate: true);
            return;
        }

        spentCoins++;
        PlayArrivalJuice();

        if (spentCoins >= requiredCoins)
        {
            CompleteUpgrade(animate: true);
            return;
        }

        RefreshVisualState(animate: true);
    }

    private void CompleteUpgrade(bool animate)
    {
        if (isCompleted)
            return;

        isCompleted = true;
        isPlayerInside = false;
        priceLabel.text = "0";
        fillTween?.Kill();

        PlayCompletionShake();

        if (animate)
        {
            fillTween = fillImage
                .DOFillAmount(1f, fillDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(FinishCompletion);
            return;
        }

        fillImage.fillAmount = 1f;
        FinishCompletion();
    }

    private void FinishCompletion()
    {
        barracksSpawner?.UpgradeLevel();
        HidePanel();
    }

    private void RefreshVisualState(bool animate)
    {
        float fillValue = Mathf.Clamp01(requiredCoins <= 0 ? 1f : spentCoins / (float)requiredCoins);

        fillTween?.Kill();

        if (animate)
        {
            fillTween = fillImage.DOFillAmount(fillValue, fillDuration).SetEase(Ease.OutBack);
        }
        else
        {
            fillImage.fillAmount = fillValue;
        }

        priceLabel.text = Mathf.Max(0, requiredCoins - spentCoins).ToString();
    }

    private void PlayArrivalJuice()
    {
        if (arrivalBurst != null)
            arrivalBurst.Play();

        if (labelPunch <= 0f)
            return;

        RectTransform labelTransform = priceLabel.rectTransform;
        labelTransform.DOComplete();
        labelTransform.DOPunchScale(Vector3.one * labelPunch, fillDuration, 1, 0.75f).SetLink(priceLabel.gameObject);
    }

    private void PlayTossSfx()
    {
        if (tossAudioSource == null || tossClip == null)
            return;

        tossStreakForPitch++;
        tossAudioSource.pitch = Mathf.Min(maxTossPitch, baseTossPitch + tossPitchStep * tossStreakForPitch);
        tossAudioSource.PlayOneShot(tossClip);
    }

    private void PlayCompletionShake()
    {
        if (completionShakeTarget == null || completionShakeStrength <= 0f)
            return;

        completionShakeTarget.DOShakePosition(completionShakeDuration, completionShakeStrength);
    }

    private void HidePanel()
    {
        panelTween = panelRoot.transform
            .DOScale(Vector3.zero, hideDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => panelRoot.SetActive(false));
    }
}
