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

    [Header("Animation")]
    [SerializeField, Min(0f)] private float showDuration = 0.18f;
    [SerializeField, Min(0f)] private float fillDuration = 0.2f;
    [SerializeField, Min(0f)] private float hideDuration = 0.2f;


    private CurrencyService currencyService;
    private Tween fillTween;
    private Tween panelTween;
    private bool isActive;
    private bool isCompleted;
    private bool isConsuming;
    private bool needsRecheck;
    private int spentCoins;

    [Inject]
    public void Construct(CurrencyService currencyService)
    {
        this.currencyService = currencyService;
    }

    private void Awake()
    {
        RefreshVisualState(animate: false);
    }

    private void OnDestroy()
    {
        UnsubscribeFromCurrency();

        fillTween?.Kill();
        panelTween?.Kill();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerView>() == null)
            return;
        
        BeginSession();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerView>() == null)
            return;

        if (!isCompleted)
            EndSession();
        panelRoot.transform.DOScale(1f, showDuration).SetEase(Ease.Linear);

    }

    private void BeginSession()
    {
        if (isCompleted || isActive)
            return;

        isActive = true;
        panelRoot.transform.DOScale(1.2f, showDuration).SetEase(Ease.OutBack);
        SubscribeToCurrency();
        TryConsumeAvailableCurrency();
    }

    private void EndSession()
    {
        if (!isActive)
            return;

        isActive = false;
        UnsubscribeFromCurrency();

        fillTween?.Kill();
        RefreshVisualState(animate: false);
    }

    private void SubscribeToCurrency()
    {
        currencyService.AmountChanged -= HandleCurrencyChanged;
        currencyService.AmountChanged += HandleCurrencyChanged;
    }

    private void UnsubscribeFromCurrency()
    {
        currencyService.AmountChanged -= HandleCurrencyChanged;
    }

    private void HandleCurrencyChanged(int amount)
    {
        if (!isActive || isCompleted)
            return;

        if (isConsuming)
        {
            needsRecheck = true;
            return;
        }

        isConsuming = true;

        do
        {
            needsRecheck = false;
            TryConsumeAvailableCurrency();
        }
        while (needsRecheck && !isCompleted);

        isConsuming = false;
    }

    private void TryConsumeAvailableCurrency()
    {
        if (isCompleted)
            return;

        int remaining = requiredCoins - spentCoins;
        if (remaining <= 0)
        {
            CompleteUpgrade(animate: true);
            return;
        }

        int spendAmount = Mathf.Min(currencyService.Amount, remaining);
        if (spendAmount <= 0)
        {
            RefreshVisualState(animate: true);
            return;
        }

        if (!currencyService.TrySpend(spendAmount))
        {
            RefreshVisualState(animate: true);
            return;
        }

        spentCoins += spendAmount;

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
        isActive = false;
        UnsubscribeFromCurrency();
        priceLabel.text = "0";
        fillTween?.Kill();

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
        SetPanelVisible();
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

    private void SetPanelVisible()
    {
        
        panelTween =  panelRoot.transform.DOScale(Vector3.zero, hideDuration).SetEase(Ease.Linear).OnComplete(() => panelRoot.SetActive(false));
    }
}
