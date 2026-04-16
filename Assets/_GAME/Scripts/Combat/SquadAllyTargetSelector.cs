using UnityEngine;

public class SquadAllyTargetSelector : IAllyTargetProvider
{
    private readonly SquadFormationRegistry soldierRegistry;

    public SquadAllyTargetSelector(SquadFormationRegistry soldierRegistry)
    {
        this.soldierRegistry = soldierRegistry;
    }

    public ICombatTarget GetBestLivingAllyTarget(Vector3 worldPosition, float reservationPenalty)
    {
        soldierRegistry.PruneInvalid();

        SoldierCombatAgent bestTarget = null;
        float bestScore = float.MaxValue;

        var soldiers = soldierRegistry.Soldiers;
        for (int i = 0; i < soldiers.Count; i++)
        {
            SoldierCombatAgent soldier = soldiers[i];
            if (soldier == null || !soldier.IsAlive)
                continue;

            int reservationCount = soldier is ITargetReservation reservationTarget
                ? reservationTarget.ReservationCount
                : 0;

            float score = CombatTargetScoringUtility.CalculateScore(
                worldPosition,
                soldier.transform.position,
                reservationCount,
                reservationPenalty);

            if (score >= bestScore)
                continue;

            bestTarget = soldier;
            bestScore = score;
        }

        if (bestTarget != null)
            return bestTarget;

        return null;
    }
}