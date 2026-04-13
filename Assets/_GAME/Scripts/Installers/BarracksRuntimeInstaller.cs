using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "BarracksRuntimeInstaller", menuName = "Installers/Combat/Barracks Runtime Installer")]
public class BarracksRuntimeInstaller : ScriptableObjectInstaller<BarracksRuntimeInstaller>
{
    [SerializeField] private BarracksStats barracksStatsTemplate = new();

    public override void InstallBindings()
    {
        var runtime = new BarracksStats(barracksStatsTemplate);
        Container.BindInstance(runtime);
    }
}
