using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "BarracksUpgradeRuntimeInstaller", menuName = "Installers/Combat/Barracks Upgrade Runtime Installer")]
public class BarracksUpgradeRuntimeInstaller : ScriptableObjectInstaller<BarracksUpgradeRuntimeInstaller>
{
    [SerializeField] private BarracksUpgradeConfig barracksUpgradeTemplate = new();

    public override void InstallBindings()
    {
        Container.BindInstance(new BarracksUpgradeConfig(barracksUpgradeTemplate));
        Container.Bind<BarracksUpgradeService>().AsSingle();
    }
}
