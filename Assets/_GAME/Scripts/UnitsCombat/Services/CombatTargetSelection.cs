using UnityEngine;

public interface ITargetReservationHandler
{
    int ReservationCount { get; }
    bool TryRegisterAttacker(ICombatTarget attacker);
    bool TryUnregisterAttacker(ICombatTarget attacker);
    void ClearReservations();
}
