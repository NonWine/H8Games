using UnityEngine;

// Unlike the other signals here this one carries a payload: the feedback that
// reacts to a hit (camera shake today, haptics or hit-stop later) needs to know
// how hard it was and where it came from, and reaching back into the hero's
// sub-container for that would couple every listener to it.
public class HeroDamagedSignal
{
    public readonly Vector3 HeroWorldPosition;
    public readonly Vector3 SourceWorldPosition;
    public readonly float Damage;
    public readonly float RemainingHealthNormalized;

    public HeroDamagedSignal(
        Vector3 heroWorldPosition,
        Vector3 sourceWorldPosition,
        float damage,
        float remainingHealthNormalized)
    {
        HeroWorldPosition = heroWorldPosition;
        SourceWorldPosition = sourceWorldPosition;
        Damage = damage;
        RemainingHealthNormalized = remainingHealthNormalized;
    }
}
