using UnityEngine;

public class CombatAnimationPresenter
{
    private readonly Animator animator;
    private float attackAnimationSpeed = 1f;

    public CombatAnimationPresenter(Animator animator)
    {
        this.animator = animator;
    }

    public void SetAttackAnimationSpeed(float speed)
    {
        attackAnimationSpeed = Mathf.Max(0.01f, speed);
    }

    public void Apply(UnitState state)
    {
        animator.speed = state == UnitState.Attack ? attackAnimationSpeed : 1f;

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
