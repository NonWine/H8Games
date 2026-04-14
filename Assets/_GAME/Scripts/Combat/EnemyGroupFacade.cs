using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class EnemyGroupFacade : MonoBehaviour
{
    [SerializeField] private Transform engagePoint;
    [SerializeField] private List<EnemyCombatAgent> enemies = new();

    public event Action<EnemyGroupFacade> Cleared;

    public EnemyGroupState State { get; private set; } = EnemyGroupState.Idle;
    public bool HasAliveMembers => HasLivingEnemies();
    public Transform EngagePoint => engagePoint != null ? engagePoint : transform;
    public Vector3 EngagePointPosition => EngagePoint.position;

    private void OnValidate()
    {
        enemies = transform.GetComponentsInChildren<EnemyCombatAgent>().ToList();
    }

    public void Activate(SquadCombatCoordinator coordinator)
    {
        State = EnemyGroupState.Activated;
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyCombatAgent enemy = enemies[i];
            if (enemy == null || !enemy.IsAlive)
                continue;

            enemy.Activate(coordinator);
        }
    }

    public ICombatTarget GetBestLivingEnemyTarget(Vector3 worldPosition, float reservationPenalty)
    {
        EnemyCombatAgent bestTarget = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyCombatAgent enemy = enemies[i];
            if (enemy == null || !enemy.IsAlive)
                continue;

            int reservationCount = enemy is ITargetReservation reservationTarget ? reservationTarget.ReservationCount : 0;
            float score = CombatTargetScoringUtility.CalculateScore(
                worldPosition,
                enemy.transform.position,
                reservationCount,
                reservationPenalty);

            if (score >= bestScore)
                continue;

            bestTarget = enemy;
            bestScore = score;
        }

        return bestTarget;
    }

    public bool ContainsEnemy(ICombatTarget target)
    {
        if (target == null)
            return false;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (ReferenceEquals(enemies[i], target))
                return enemies[i] != null && enemies[i].IsAlive;
        }

        return false;
    }

    private bool HasLivingEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null && enemies[i].IsAlive)
                return true;
        }

        return false;
    }

    public void ResetRuntimeState()
    {
        State = EnemyGroupState.Idle;
        RefreshEnemies();

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyCombatAgent enemy = enemies[i];
            if (enemy == null)
                continue;

            enemy.ResetRunTimeState();
        }
    }

    private void RefreshEnemies()
    {

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyCombatAgent enemy = enemies[i];
            if (enemy == null)
                continue;

            enemy.SetGroup(this);
            enemy.Died += HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(EnemyCombatAgent enemy)
    {
        enemy.Died -= HandleEnemyDied;
        TryMarkCleared();

    }

    private void TryMarkCleared()
    {
        if (State == EnemyGroupState.Cleared || HasAliveMembers)
            return;

        State = EnemyGroupState.Cleared;
        Cleared?.Invoke(this);
    }
}
