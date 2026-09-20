using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class EnemyGroupViewController : MonoBehaviour
{
    // Where each enemy was authored, captured the first time the group resolves its
    // controllers - before anything can move or kill them. A reset warps them back
    // here, so an enemy that chased the squad across the level does not respawn at
    // the spot it happened to die on.
    private readonly struct EnemySpawnPose
    {
        public EnemySpawnPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }

    [SerializeField] private Transform engagePoint;
    [SerializeField] private List<BaseCombatAgentView> enemyViews = new();

    private readonly List<EnemyCombatAgentController> enemyControllers = new();
    private readonly List<EnemySpawnPose> enemySpawnPoses = new();

    public event Action<EnemyGroupViewController> Cleared;

    public EnemyGroupState State { get; private set; } = EnemyGroupState.Idle;
    public bool HasAliveMembers => HasLivingEnemies();
    public Transform EngagePoint => engagePoint != null ? engagePoint : transform;
    public Vector3 EngagePointPosition => EngagePoint.position;
    public IReadOnlyList<EnemyCombatAgentController> Enemies
    {
        get
        {
            CacheControllers();
            return enemyControllers;
        }
    }

    private void Start()
    {
        CacheControllers();
    }

    private void OnValidate()
    {
        enemyViews = GetComponentsInChildren<BaseCombatAgentView>().ToList();
    }

    public void Activate()
    {
        CacheControllers();
        State = EnemyGroupState.Activated;
    }

    public bool ContainsEnemy(ICombatTarget target)
    {
        if (target == null)
        {
            return false;
        }

        CacheControllers();

        for (int i = 0; i < enemyControllers.Count; i++)
        {
            EnemyCombatAgentController enemy = enemyControllers[i];
            if (enemy == null)
            {
                continue;
            }

            if (ReferenceEquals(enemy, target))
            {
                return enemy.IsAlive;
            }
        }

        return false;
    }

    // Flipping State back to Idle is not enough on its own: the squad only marches
    // at a group that still has someone standing, so a group whose members stayed
    // dead was skipped forever and the Start button silently did nothing.
    public void ResetRuntimeState()
    {
        State = EnemyGroupState.Idle;
        RefreshEnemies();
        RespawnEnemies();
    }

    private bool HasLivingEnemies()
    {
        CacheControllers();

        for (int i = 0; i < enemyControllers.Count; i++)
        {
            if (enemyControllers[i] != null && enemyControllers[i].IsAlive)
            {
                return true;
            }
        }

        return false;
    }

    private void CacheControllers()
    {
        if (enemyViews == null || enemyViews.Count == 0 || enemyControllers.Count == enemyViews.Count)
        {
            return;
        }

        enemyControllers.Clear();
        enemySpawnPoses.Clear();

        for (int i = 0; i < enemyViews.Count; i++)
        {
            BaseCombatAgentView enemyView = enemyViews[i];
            if (enemyView == null)
            {
                continue;
            }

            GameObjectContext context = enemyView.GetComponent<GameObjectContext>();
            if (context == null || context.Container == null)
            {
                continue;
            }

            try
            {
                EnemyCombatAgentController controller = context.Container.Resolve<EnemyCombatAgentController>();
                if (controller is not EnemyCombatAgentController enemyController)
                {
                    Debug.LogError($"[EnemyGroupViewController] Controller on '{enemyView.name}' is not an enemy controller.");
                    continue;
                }

                // A view that never resolves keeps the counts apart, so this method
                // re-runs on every call. Drop the old handler before re-adding it or
                // the survivors collect one extra Cleared trigger per pass.
                enemyController.Died -= HandleEnemyDied;
                enemyController.Died += HandleEnemyDied;
                enemyControllers.Add(enemyController);
                enemySpawnPoses.Add(new EnemySpawnPose(enemyView.transform.position, enemyView.transform.rotation));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[EnemyGroupViewController] Could not resolve controller for '{enemyView.name}': {exception.Message}");
            }
        }
    }

    private void RefreshEnemies()
    {
        CacheControllers();

        for (int i = 0; i < enemyControllers.Count; i++)
        {
            EnemyCombatAgentController enemy = enemyControllers[i];
            if (enemy == null)
            {
                continue;
            }

            enemy.Died -= HandleEnemyDied;
            enemy.Died += HandleEnemyDied;
        }
    }

    // Only the fallen are rebuilt. Survivors keep the damage they took, so a retry
    // does not quietly heal the half-cleared group the player just lost to.
    private void RespawnEnemies()
    {
        for (int i = 0; i < enemyControllers.Count; i++)
        {
            EnemyCombatAgentController enemy = enemyControllers[i];
            if (enemy == null || enemy.IsAlive || i >= enemySpawnPoses.Count)
            {
                continue;
            }

            EnemySpawnPose pose = enemySpawnPoses[i];
            enemy.Spawn(pose.Position, pose.Rotation);
        }
    }

    private void HandleEnemyDied()
    {
        TryMarkCleared();
    }

    private void TryMarkCleared()
    {
        if (State == EnemyGroupState.Cleared || HasAliveMembers)
        {
            return;
        }

        State = EnemyGroupState.Cleared;
        Cleared?.Invoke(this);
    }
}
