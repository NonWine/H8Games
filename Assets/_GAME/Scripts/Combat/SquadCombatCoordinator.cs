using System.Collections.Generic;
using UnityEngine;
using Zenject;

public sealed class SquadCombatCoordinator : MonoBehaviour
{
    [SerializeField] private float detectorRadius;
    
    [SerializeField] private List<SoldierCombatAgent> soldiers = new();
    
    private bool squadDefeatedRaised;
    [InjectOptional] private PlayerView heroTarget;

    public event System.Action<EnemyGroupFacade> EncounterStarted;
    public event System.Action<EnemyGroupFacade> EncounterCleared;
    public event System.Action SquadDefeated;
    public EnemyGroupFacade CurrentTargetGroup { get; private set; }
    public bool HasActiveEncounter => CurrentTargetGroup != null && CurrentTargetGroup.State == EnemyGroupState.Activated;
    public bool HasLivingAllies
    {
        get
        {
            PruneSoldiers();
            return soldiers.Count > 0;
        }
    }

    public void RegisterSoldier(SoldierCombatAgent soldier)
    {
        if (soldier == null || soldiers.Contains(soldier))
            return;

        soldiers.Add(soldier);
        squadDefeatedRaised = false;
    }

    public void UnregisterSoldier(SoldierCombatAgent soldier)
    {
        if (soldier == null)
            return;

        soldiers.Remove(soldier);
        TryRaiseSquadDefeated();
    }

    public bool TryBeginEncounter(EnemyGroupFacade enemyGroup)
    {
        if (enemyGroup == null || !enemyGroup.IsAvailableForEncounter)
            return false;

        if (CurrentTargetGroup != null)
            return CurrentTargetGroup == enemyGroup;

        CurrentTargetGroup = enemyGroup;
        CurrentTargetGroup.Cleared += HandleEncounterCleared;
        EncounterStarted?.Invoke(enemyGroup);
        enemyGroup.Activate(this);
        return true;
    }

    public ICombatTarget GetClosestLivingAlly(Vector3 worldPosition)
    {
        PruneSoldiers();

        SoldierCombatAgent closest = null;
        float closestSqrDistance = float.MaxValue;
        
        for (int i = 0; i < soldiers.Count; i++)
        {
            SoldierCombatAgent soldier = soldiers[i];
            Vector3 delta = soldier.transform.position - worldPosition;
            delta.y = 0f;
            float sqrDistance = delta.sqrMagnitude;
            if (sqrDistance >= closestSqrDistance)
                continue;

            closest = soldier;
            closestSqrDistance = sqrDistance;
        }

        return closest;
    }

    public ICombatTarget GetBestLivingAllyTarget(Vector3 worldPosition, float reservationPenalty)
    {
        PruneSoldiers();

        SoldierCombatAgent bestTarget = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < soldiers.Count; i++)
        {
            SoldierCombatAgent soldier = soldiers[i];
            if (soldier == null || !soldier.IsAlive)
                continue;

            int reservationCount = soldier is ITargetReservation reservationTarget ? reservationTarget.ReservationCount : 0;
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

        if (heroTarget != null && heroTarget.IsAlive)
            return heroTarget;

        return null;
    }

    private void HandleEncounterCleared(EnemyGroupFacade clearedGroup)
    {
        if (CurrentTargetGroup != clearedGroup)
            return;

        clearedGroup.Cleared -= HandleEncounterCleared;
        CurrentTargetGroup = null;
        EncounterCleared?.Invoke(clearedGroup);
    }

    private void PruneSoldiers()
    {
        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            SoldierCombatAgent soldier = soldiers[i];
            if (soldier != null && soldier.IsAlive)
                continue;

            soldiers.RemoveAt(i);
        }
    }

    private void TryRaiseSquadDefeated()
    {
        if (squadDefeatedRaised)
            return;

        if (CurrentTargetGroup == null)
            return;

        if (HasLivingAllies)
            return;

        squadDefeatedRaised = true;
        SquadDefeated?.Invoke();
    }
}
