using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class HeroCameraInstaller : MonoInstaller
{
    [SerializeField] private CinemachineCamera followCamera;
    [SerializeField] private CinemachineCameraShaker cameraShaker;
    [SerializeField] private CameraShakeConfig cameraShakeConfig;
    [SerializeField] private CombatCameraFocusConfig combatFocusConfig;

    public override void InstallBindings()
    {
        InstallCameraBindings();
        InstallShakeBindings();
        InstallCombatFocusBindings();
    }

    private void InstallCameraBindings()
    {
        Container.Bind<CinemachineCamera>().FromInstance(followCamera).AsSingle();
        Container.BindInterfacesAndSelfTo<HeroCameraController>().AsSingle();
    }

    private void InstallCombatFocusBindings()
    {
        Container.Bind<CombatCameraFocusConfig>().FromInstance(combatFocusConfig).AsSingle();
    }

    // The shaker is a scene component, so the container only wraps it as the
    // shake service; SceneContext is what pushes the config into it via [Inject].
    private void InstallShakeBindings()
    {
        Container.Bind<CameraShakeConfig>().FromInstance(cameraShakeConfig).AsSingle();
        Container.Bind<ICameraShakeService>().FromInstance(cameraShaker).AsSingle();
        Container.BindInterfacesTo<HeroDamageCameraShakeListener>().AsSingle().NonLazy();
        Container.BindInterfacesTo<CombatCameraShakeListener>().AsSingle().NonLazy();
    }
}
