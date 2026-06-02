using Unity.Cinemachine;
using Zenject;

public class HeroCameraController : IInitializable
{
    private readonly PlayerView hero;
    private readonly CinemachineCamera followCamera;

    public HeroCameraController(PlayerView hero, CinemachineCamera followCamera)
    {
        this.hero = hero;
        this.followCamera = followCamera;
    }

    public void Initialize()
    {
        followCamera.Follow = hero.CameraAnchor;
        followCamera.LookAt = hero.CameraAnchor;
    }
}
