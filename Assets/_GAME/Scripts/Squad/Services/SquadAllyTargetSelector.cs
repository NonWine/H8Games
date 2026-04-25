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

        SoldierCombatAgentController bestTarget = null;
        float bestScore = float.MaxValue;

        var soldiers = soldierRegistry.Soldiers;
        for (int i = 0; i < soldiers.Count; i++)
        {
            SoldierCombatAgentController soldier = soldiers[i];
            if (soldier == null || !soldier.IsAlive)
            {
                continue;
            }

            int reservationCount = soldier.reservationHandler.ReservationCount;

            float score = CombatTargetScoringUtility.CalculateScore(
                worldPosition,
                soldier.transform.position,
                reservationCount,
                reservationPenalty);

            if (score >= bestScore)
            {
                continue;
            }

            bestTarget = soldier;
            bestScore = score;
        }

        return bestTarget;
    }
}
