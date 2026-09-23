using UnityEngine;

// A soldier or an enemy went down. The hero is deliberately not included: his
// death already has HeroDefeatedSignal, and firing both would double every
// reaction to it.
public class UnitDiedSignal
{
    public readonly Vector3 WorldPosition;
    public readonly CombatSide Side;

    public UnitDiedSignal(Vector3 worldPosition, CombatSide side)
    {
        WorldPosition = worldPosition;
        Side = side;
    }
}
