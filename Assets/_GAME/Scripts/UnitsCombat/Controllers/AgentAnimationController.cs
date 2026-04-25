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
                animator.SetInteger("State", 0);
                break;
            case UnitState.Move:
                animator.SetInteger("State", 1);
                break;
            case UnitState.Attack:
                animator.SetInteger("State", 2);
                break;
            case UnitState.Dead:
                animator.SetInteger("State", 3);
                break;
        }
    }

    public void Reset()
    {
        animator.speed = 1f;
        animator.SetInteger("State", 0);
    }
    
}
