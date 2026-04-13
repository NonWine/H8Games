using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class HeroCameraInstaller : MonoInstaller
{
    [SerializeField] private CinemachineCamera followCamera;

    public override void InstallBindings()
    {
        InstallCameraBindings();
    }
    
    private void InstallCameraBindings()
    {
        Container.Bind<CinemachineCamera>().FromInstance(followCamera).AsSingle();
        Container.BindInterfacesAndSelfTo<HeroCameraController>().AsSingle();
    }
}