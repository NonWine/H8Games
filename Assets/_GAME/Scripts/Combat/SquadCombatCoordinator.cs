using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public sealed class SquadCombatCoordinator : MonoBehaviour
{
    [SerializeField] private float detectorRadius;
    
    [SerializeField] private List<SoldierCombatAgent> soldiers = new();
    
    private bool squadDefeatedRaised;
    [InjectOptional] private PlayerView heroTarget;

    public event Action<EnemyGroupFacade> CombatStartedBattle;
    public event Action<EnemyGroupFacade> CombatClearedZone;
    public event Action SquadDefeated;
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

    public void TryBeginEncounter(EnemyGroupFacade enemyGroup)
    {
        CurrentTargetGroup = enemyGroup;
        CurrentTargetGroup.Cleared += HandleEncounterCleared;
        CombatStartedBattle?.Invoke(enemyGroup);
        enemyGroup.Activate(this);
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
            float score = CombatTargetScoringUtility.CalculateScore(worldPosition, soldier.transform.position, reservationCount, reservationPenalty);

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
        
        clearedGroup.Cleared -= HandleEncounterCleared;
        CombatClearedZone?.Invoke(clearedGroup);
        CurrentTargetGroup = null;
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
