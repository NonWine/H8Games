using UnityEngine;

// Every hit landed on any combat unit, hero included. Feedback only - nothing in
// the combat rules listens to this, so a listener can be added or removed
// without touching a single unit.
//
// HeroDamagedSignal still exists alongside it and is not replaced: that one
// carries the attacker's position, which is what the camera shake needs to push
// the camera the way the blow travelled. This one is about the hit itself.
public class UnitDamagedSignal
{
    public readonly Vector3 WorldPosition;
    public readonly float Damage;
    public readonly CombatSide Side;
    public readonly bool WasLethal;

    public UnitDamagedSignal(Vector3 worldPosition, float damage, CombatSide side, bool wasLethal)
    {
        WorldPosition = worldPosition;
        Damage = damage;
        Side = side;
        WasLethal = wasLethal;
    }
}
