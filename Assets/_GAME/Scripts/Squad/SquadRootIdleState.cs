public class SquadRootIdleState : SquadRootStateBase
{
    private readonly SquadHomeController homeController;

    public override void Enter()
    {
        homeController.SnapToHome();

    }

    public override void Exit()
    {
    }
}