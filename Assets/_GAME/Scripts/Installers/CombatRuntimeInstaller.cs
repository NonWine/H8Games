using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "CombatRuntimeInstaller", menuName = "Installers/Combat/Runtime Installer")]
public class CombatRuntimeInstaller : ScriptableObjectInstaller<CombatRuntimeInstaller>
{
    [SerializeField] private HeroRuntimeInstaller heroRuntimeInstaller;
    [SerializeField] private BarracksRuntimeInstaller barracksRuntimeInstaller;
    [SerializeField] private HeroUpgradeRuntimeInstaller heroUpgradeRuntimeInstaller;
    [SerializeField] private BarracksUpgradeRuntimeInstaller barracksUpgradeRuntimeInstaller;

    public override void InstallBindings()
    {
        InstallChild(heroRuntimeInstaller);
        InstallChild(barracksRuntimeInstaller);
        InstallChild(heroUpgradeRuntimeInstaller);
        InstallChild(barracksUpgradeRuntimeInstaller);
    }

    private void InstallChild(ScriptableObjectInstallerBase installer)
    {
        if (installer == null)
            return;

        Container.Inject(installer);
        installer.InstallBindings();
    }
}
