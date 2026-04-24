using System.Collections.Generic;
using UnityEngine;

public class TargetReservation : ITargetReservation, IResetModule, IDisposeModule
{
    public int ReservationCount => reservationAttackers.Count;
 
    private readonly HashSet<ICombatTarget> reservationAttackers = new();

    public bool TryRegisterAttacker(ICombatTarget attacker)
    {
        if (attacker == null)
            return false;

        return reservationAttackers.Add(attacker);    
    }

    public bool TryUnregisterAttacker(ICombatTarget attacker)
    {
        if (attacker == null)
            return false;

        return reservationAttackers.Remove(attacker);    
    }

    public void ClearReservations()
    {
        reservationAttackers.Clear();
    }

    public void Reset()
    {
        ClearReservations();
    }

    public void Dispose()
    {
        ClearReservations();
    }
}
