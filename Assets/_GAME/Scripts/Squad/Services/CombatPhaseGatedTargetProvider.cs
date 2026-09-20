// A soldier senses for enemies only while the squad is actually out on an
// encounter. Sensing around the clock made men walking back to their formation
// slot lock onto whatever stood inside their DetectionRadius: the move state
// handed them to the attack state, the attack state handed them straight back,
// and they spun on the spot playing the run animation instead of settling in.
// Outside MovingToZone and FightingZone there is no target to hand out, so the
// plain "walk to my slot, then hold it" path is all that is left.
public class CombatPhaseGatedTargetProvider : ICombatTargetProvider
{
    private readonly ICombatTargetProvider innerProvider;
    private readonly ICombatStateProvider combatStateProvider;

    public CombatPhaseGatedTargetProvider(
        ICombatTargetProvider innerProvider,
        ICombatStateProvider combatStateProvider)
    {
        this.innerProvider = innerProvider;
        this.combatStateProvider = combatStateProvider;
    }

    public ICombatTarget GetTarget()
    {
        return IsEngagementPhase() ? innerProvider.GetTarget() : null;
    }

    private bool IsEngagementPhase()
    {
        CombatFlowState state = combatStateProvider.State;

        return state == CombatFlowState.MovingToZone || state == CombatFlowState.FightingZone;
    }
}
