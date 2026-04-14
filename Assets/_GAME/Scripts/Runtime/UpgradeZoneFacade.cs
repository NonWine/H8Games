using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class UpgradeZoneFacade : MonoBehaviour
{
    [SerializeField] private UpgradeKind upgradeKind;
    [SerializeField] private PlayerView heroTarget;
    [SerializeField] private UpgradeZoneView view;

    private CurrencyService currencyService;
    private UpgradePriceService upgradePriceService;
    private PlayerView heroInRange;

    [Inject]
    public void Construct(CurrencyService currencyService, UpgradePriceService upgradePriceService)
    {
        this.currencyService = currencyService;
        this.upgradePriceService = upgradePriceService;
        RefreshView();
    }

    private void Update()
    {
        if (heroInRange == null || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        TryUpgrade();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerView hero))
            return;

        heroInRange = hero;
        RefreshView();
        view?.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerView hero) || hero != heroInRange)
            return;

        heroInRange = null;
        view?.Hide();
    }

    private void TryUpgrade()
    {
        bool purchased = false;
        PlayerView target = heroTarget != null ? heroTarget : heroInRange;
        if (IsHeroUpgrade() && target != null)
            purchased = target.UpgradeService.TryUpgrade(upgradeKind, currencyService, upgradePriceService, out _);

        if (purchased)
            RefreshView();
    }

    private bool IsHeroUpgrade()
    {
        return upgradeKind == UpgradeKind.HeroDamage || upgradeKind == UpgradeKind.HeroMaxHealth ||
               upgradeKind == UpgradeKind.HeroAttackRate;
    }

    private int GetCurrentLevel()
    {
        if (IsHeroUpgrade())
        {
            PlayerView target = heroTarget != null ? heroTarget : heroInRange;
            return target != null ? target.UpgradeService.GetLevel(upgradeKind) : 0;
        }

        return 0;
    }

    private void RefreshView()
    {
        int level = GetCurrentLevel();
        UpgradeDefinition definition = GetDefinition();
        int price = definition != null && upgradePriceService != null
            ? upgradePriceService.GetPrice(definition, level)
            : 0;
        int maxLevel = definition?.MaxLevel ?? 0;
        view?.SetState(upgradeKind.ToString(), price, level, maxLevel);
    }

    private UpgradeDefinition GetDefinition()
    {
        if (IsHeroUpgrade())
        {
            PlayerView target = heroTarget != null ? heroTarget : heroInRange;
            return target != null ? target.UpgradeService.GetDefinition(upgradeKind) : null;
        }

        return null;
    }
}
