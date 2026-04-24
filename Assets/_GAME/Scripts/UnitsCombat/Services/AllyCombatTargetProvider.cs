using UnityEngine;

public class AllyCombatTargetProvider : ICombatTargetProvider
{
    private readonly Transform ownerTransform;
    private readonly float reservationPenalty;
    private readonly IAllyTargetProvider allyTargetProvider;

    public AllyCombatTargetProvider(
        Transform ownerTransform,
        float reservationPenalty,
        IAllyTargetProvider allyTargetProvider)
    {
        this.ownerTransform = ownerTransform;
        this.reservationPenalty = reservationPenalty;
        this.allyTargetProvider = allyTargetProvider;
    }

    public ICombatTarget GetTarget()
    {
        return allyTargetProvider.GetBestLivingAllyTarget(ownerTransform.position, reservationPenalty);
    }
}
