using TMPro;
using UnityEngine;
using Zenject;

public class CurrencyCounterView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountLabel;

    private CurrencyService currencyService;

    [Inject]
    public void Construct(CurrencyService currencyService)
    {
        this.currencyService = currencyService;
        this.currencyService.AmountChanged += HandleAmountChanged;
        HandleAmountChanged(this.currencyService.Amount);
    }

    private void OnDestroy()
    {
        if (currencyService != null)
            currencyService.AmountChanged -= HandleAmountChanged;
    }

    private void HandleAmountChanged(int amount)
    {
        if (amountLabel != null)
            amountLabel.text = amount.ToString();
    }
}
