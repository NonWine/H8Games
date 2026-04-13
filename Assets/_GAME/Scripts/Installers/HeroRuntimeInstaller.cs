using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "HeroRuntimeInstaller", menuName = "Installers/Combat/Hero Runtime Installer")]
public class HeroRuntimeInstaller : ScriptableObjectInstaller<HeroRuntimeInstaller>
{
    [SerializeField] private HeroStats heroStatsTemplate = new();

    public override void InstallBindings()
    {
        Container.BindInstance(new HeroStats(heroStatsTemplate));
    }
}
