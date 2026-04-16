using UnityEngine;

public class CombatAnimationPresenter
{
    private readonly Animator animator;

    public CombatAnimationPresenter(Animator animator)
    {
        this.animator = animator;
    }

    public void Apply(UnitState state)
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
}