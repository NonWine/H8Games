// Idle has to actively stop the mover, not merely represent standing still.
// SquadMoveProvider keeps translating the root every tick until something calls
// Stop(), so an Enter() that did nothing let the squad walk on while the flow
// believed it was parked - which is how a defeat left the formation drifting
// toward the enemy group it had just lost to.
public class SquadRootIdleState : SquadRootStateBase
{
    private readonly IMoveProvider squadMoveProvider;

    public SquadRootIdleState(IMoveProvider squadMoveProvider)
    {
        this.squadMoveProvider = squadMoveProvider;
    }

    public override void Enter()
    {
        squadMoveProvider.Stop();
    }

    public override void Exit()
    {
    }
}
