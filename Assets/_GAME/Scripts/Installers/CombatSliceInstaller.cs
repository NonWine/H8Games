using UnityEngine;
using Zenject;

public class CombatSliceInstaller : MonoInstaller
{
    [SerializeField] private CombatRuntimeInstaller runtimeInstaller;

    public override void InstallBindings()
    {
        if (runtimeInstaller != null)
            InstallChild(runtimeInstaller);
        else
            InstallFallbackRuntimeData();

        Container.Bind<CurrencyService>().AsSingle();
        Container.Bind<UpgradePriceService>().AsSingle();
        Container.Bind<CombatEndService>().AsSingle();
    }

    private void InstallChild(ScriptableObjectInstallerBase installer)
    {
        Container.Inject(installer);
        installer.InstallBindings();
    }

    private void InstallFallbackRuntimeData()
    {
        Container.BindInstance(new HeroStats());
        Container.BindInstance(new BarracksStats());

        BindFallbackUpgrade(UpgradeKind.HeroDamage);
        BindFallbackUpgrade(UpgradeKind.HeroMaxHealth);
        BindFallbackUpgrade(UpgradeKind.HeroAttackRate);
        BindFallbackUpgrade(UpgradeKind.BarracksSpawnSpeed);
        BindFallbackUpgrade(UpgradeKind.BarracksUnitHealth);
        BindFallbackUpgrade(UpgradeKind.BarracksUnitDamage);
    }

    private void BindFallbackUpgrade(UpgradeKind kind)
    {
        Container.BindInstance(new UpgradeDefinition()).WithId(kind);
    }
}
