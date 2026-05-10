using System;
using UnityEngine;

public class AgentAnimationController
{

    private readonly Animator animator;

    public AgentAnimationController(Animator animator)
    {
       this.animator = animator;

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
        animator.speed = 1f;
        animator.SetBool("IsAttacking", false);
        animator.SetInteger("State", 0);
    }
    
}
