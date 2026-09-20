using System;
using UnityEngine;

public class AgentAnimationController
{
    private const float MinPlaybackSpeedMultiplier = 0.88f;
    private const float MaxPlaybackSpeedMultiplier = 1.12f;

    private readonly Animator animator;
    private readonly float playbackSpeedMultiplier;

    public AgentAnimationController(Animator animator)
    {
        this.animator = animator;

        // Per-unit playback speed: identical walk cycles running at exactly the
        // same rate is what makes a squad read as marching robots.
        playbackSpeedMultiplier = Mathf.Lerp(
            MinPlaybackSpeedMultiplier,
            MaxPlaybackSpeedMultiplier,
            DeterministicHashUtility.Hash01(animator.GetInstanceID()));

        animator.speed = playbackSpeedMultiplier;
    }

    public void SetAnimationState(UnitState state)
    {
        switch (state)
        {
            case UnitState.Idle:
                animator.SetBool("IsAttacking", false);
                animator.SetInteger("State", 0);
                break;
            case UnitState.Move:
                animator.SetBool("IsAttacking", false);
                animator.SetInteger("State", 1);
                break;
            case UnitState.Attack:
                animator.SetInteger("State", 0);
                animator.SetBool("IsAttacking", true);
                break;
            case UnitState.Dead:
                animator.SetBool("IsAttacking", false);
                animator.SetInteger("State", 3);
                break;
        }
    }
    
    public void SetTrig(string name) => animator.SetTrigger(name);

    public void SetAttackTrigger() => SetTrig("AttackTrigger");
    
    public void Reset()
    {
        animator.speed = playbackSpeedMultiplier;
        animator.SetBool("IsAttacking", false);
        animator.SetInteger("State", 0);
    }
    
}
