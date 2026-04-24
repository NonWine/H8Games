using UnityEngine;

public static class CombatTargetScoringUtility
{
    public static float CalculateScore(
        Vector3 attackerPosition,
        Vector3 targetPosition,
        int assignedAttackersCount,
        float reservationPenalty)
    {
        Vector3 delta = targetPosition - attackerPosition;
        delta.y = 0f;

        float distance = delta.magnitude;
        float penalty = Mathf.Max(0f, assignedAttackersCount) * Mathf.Max(0f, reservationPenalty);
        return distance + penalty;
    }
}
