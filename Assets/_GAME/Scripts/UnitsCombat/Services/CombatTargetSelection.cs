using UnityEngine;

public interface ITargetReservation
{
    int ReservationCount { get; }
    bool TryRegisterAttacker(ICombatTarget attacker);
    bool TryUnregisterAttacker(ICombatTarget attacker);
    void ClearReservations();
}
