using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "CombatRuntimeInstaller", menuName = "Installers/Combat/Runtime Installer")]
public class CombatRuntimeInstaller : ScriptableObjectInstaller<CombatRuntimeInstaller>
{
    [SerializeField] private HeroStats heroStatsTemplate = new();
    [SerializeField] private BarracksStats barracksStatsTemplate = new();
    [SerializeField] private UpgradeDefinition heroDamageUpgradeTemplate = new();
    [SerializeField] private UpgradeDefinition heroMaxHealthUpgradeTemplate = new();
    [SerializeField] private UpgradeDefinition heroAttackRateUpgradeTemplate = new();
    [SerializeField] private UpgradeDefinition barracksSpawnSpeedUpgradeTemplate = new();
    [SerializeField] private UpgradeDefinition barracksUnitHealthUpgradeTemplate = new();
    [SerializeField] private UpgradeDefinition barracksUnitDamageUpgradeTemplate = new();

    public override void InstallBindings()
    {
        Container.BindInstance(new HeroStats(heroStatsTemplate));
        Container.BindInstance(new BarracksStats(barracksStatsTemplate));

        BindUpgrade(UpgradeKind.HeroDamage, heroDamageUpgradeTemplate);
        BindUpgrade(UpgradeKind.HeroMaxHealth, heroMaxHealthUpgradeTemplate);
        BindUpgrade(UpgradeKind.HeroAttackRate, heroAttackRateUpgradeTemplate);
        BindUpgrade(UpgradeKind.BarracksSpawnSpeed, barracksSpawnSpeedUpgradeTemplate);
        BindUpgrade(UpgradeKind.BarracksUnitHealth, barracksUnitHealthUpgradeTemplate);
        BindUpgrade(UpgradeKind.BarracksUnitDamage, barracksUnitDamageUpgradeTemplate);
    }

    private void BindUpgrade(UpgradeKind kind, UpgradeDefinition template)
    {
        Container.BindInstance(template != null ? new UpgradeDefinition(template) : new UpgradeDefinition())
            .WithId(kind);
    }
}
