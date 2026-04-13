using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "HeroUpgradeRuntimeInstaller", menuName = "Installers/Combat/Hero Upgrade Runtime Installer")]
public class HeroUpgradeRuntimeInstaller : ScriptableObjectInstaller<HeroUpgradeRuntimeInstaller>
{
    [SerializeField] private HeroUpgradeConfig heroUpgradeTemplate = new();

    public override void InstallBindings()
    {
        Container.BindInstance(new HeroUpgradeConfig(heroUpgradeTemplate));
    }
}
