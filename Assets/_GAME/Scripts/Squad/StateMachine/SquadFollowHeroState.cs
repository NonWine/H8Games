using UnityEngine;
using Zenject;

public class SquadFollowHeroState : SquadRootStateBase
{
    private readonly PlayerView playerView;
    private readonly SquadFollowSettings settings;
    private readonly IDestinationProvider enemyDestination;
    private readonly SquadMoveProvider squadMoveProvider;

    private readonly IAliveStateReader aliveState;

    public SquadFollowHeroState(
        PlayerView playerView,
        SquadFollowSettings settings,
        IDestinationProvider enemyDestination,
        SquadMoveProvider squadMoveProvider,
        [InjectOptional] IAliveStateReader aliveState = null)
    {
        this.playerView = playerView;
        this.settings = settings;
        this.enemyDestination = enemyDestination;
        this.squadMoveProvider = squadMoveProvider;
        this.aliveState = aliveState;
    }

    public override void Enter()
    {
        squadMoveProvider.Stop();
    }

    public override void Exit()
    {
        squadMoveProvider.SetExternalMoving(false);
    }

    public override void Tick()
    {
        if (playerView == null || RootView == null)
        {
            squadMoveProvider.SetExternalMoving(false);
            return;
        }

        if (aliveState != null && !aliveState.IsAlive)
        {
            squadMoveProvider.SetExternalMoving(false);
            return;
        }

        Vector3 heroPos = playerView.transform.position;
        heroPos.y = RootView.transform.position.y;

        Vector3 delta = heroPos - RootView.transform.position;
        delta.y = 0f;

        float distance = delta.magnitude;
        bool isMoving = distance > 0.15f;
        squadMoveProvider.SetExternalMoving(isMoving);

        // Dynamic follow speed: matches hero move speed or speeds up to catch up if lagging behind
        float followSpeed = Mathf.Max(settings.RootMoveSpeed, distance * 2.5f);
        RootView.transform.position = Vector3.MoveTowards(
            RootView.transform.position,
            heroPos,
            followSpeed * Time.deltaTime);

        // Orient formation towards enemies so slots (which have negative Z offsets) fall behind the hero facing forward
        Vector3 aimDirection = Vector3.zero;
        if (enemyDestination != null && enemyDestination.HasDestination)
        {
            aimDirection = enemyDestination.Destination - heroPos;
        }
        else
        {
            aimDirection = playerView.transform.forward;
        }

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection.normalized, Vector3.up);
            float lerpFactor = 1f - Mathf.Exp(-settings.RootFollowSmoothness * Time.deltaTime);
            RootView.transform.rotation = Quaternion.Slerp(RootView.transform.rotation, targetRotation, lerpFactor);
        }
    }
}
