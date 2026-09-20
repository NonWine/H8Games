using UnityEngine;

// The hero rig uses its own animator controller ("Player"), not the combat one:
// it exposes Speed/MotionSpeed/State and has no IsAttacking bool or AttackTrigger.
// Driving it through AgentAnimationController would log missing-parameter warnings
// every attack and map Dead onto the worker "Digging" clip, so the hero gets its
// own adapter. Locomotion stays owned by PlayerController's Speed float.
public class HeroAnimationController
{
    private const string StateParameter = "State";

    private const int IdleStateValue = 0;
    private const int AttackStateValue = 2;
    private const int DeadStateValue = 3;

    private readonly Animator animator;

    public HeroAnimationController(Animator animator)
    {
        this.animator = animator;
    }

    public void PlayIdle()
    {
        animator.SetInteger(StateParameter, IdleStateValue);
    }

    public void PlayAttack()
    {
        animator.SetInteger(StateParameter, AttackStateValue);
    }


    public void PlayDead()
    {
        animator.SetInteger(StateParameter, DeadStateValue);
    }
}
