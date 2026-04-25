using System;
using UnityEngine;

public class CombatAnimationPresenter : ICombatTickModule, IResetModule
{
    private readonly BaseCombatUnitView view;

    private readonly Animator animator;


    public CombatAnimationPresenter(BaseCombatUnitView view)
    {
        this.view = view;
        animator = view.Animator;

    }

    public void Tick(UnitState state, float deltaTime)
    {
        
        switch (state)
        {
            case UnitState.Idle:
                animator.SetInteger("State", 0);
                break;
            case UnitState.Move:
            case UnitState.Chase:
                animator.SetInteger("State", 1);
                break;
            case UnitState.Attack:
                animator.SetInteger("State", 2);
                break;
            case UnitState.Stunned:
                animator.SetInteger("State", 0);
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
