using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

public class CurrencyCounterView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountLabel;
    [SerializeField, Min(0f)] private float scalePunch = 0.08f;
    [SerializeField, Min(0f)] private float punchDuration = 0.18f;

    private CurrencyService currencyService;
    private Vector3 amountLabelBaseScale;
    private bool hasCachedBaseScale;

    private void Awake()
    {
        CacheBaseScale();
    }

    [Inject]
    public void Construct(CurrencyService currencyService)
    {
        this.currencyService = currencyService;
        this.currencyService.AmountChanged += HandleAmountChanged;
        HandleAmountChanged(this.currencyService.Amount, false);
    }

    private void OnDestroy()
    {
        if (currencyService != null)
            currencyService.AmountChanged -= HandleAmountChanged;
    }

    private void HandleAmountChanged(int amount)
    {
        HandleAmountChanged(amount, true);
    }

    private void HandleAmountChanged(int amount, bool animate)
    {
        if (amountLabel == null)
            return;

        amountLabel.text = amount.ToString();
        CacheBaseScale();

        if (!animate)
        {
            amountLabel.rectTransform.localScale = amountLabelBaseScale;
            return;
        }

        RectTransform labelTransform = amountLabel.rectTransform;
        labelTransform.DOKill();
        labelTransform.localScale = amountLabelBaseScale;
        labelTransform
            .DOPunchScale(Vector3.one * scalePunch, punchDuration, 1, 0.75f)
            .SetLink(gameObject);
    }

    private void CacheBaseScale()
    {
        if (hasCachedBaseScale || amountLabel == null)
            return;

        amountLabelBaseScale = amountLabel.rectTransform.localScale;
        hasCachedBaseScale = true;
    }
}
