using UnityEngine;

public sealed class EnemyCombatAgent : MonoBehaviour
{
    private StaticEnemyAgent owner;
    private SquadCombatCoordinator squadCombatCoordinator;
    private AttackService attackService;
    private ICombatTarget currentTarget;

    public void Initialize(StaticEnemyAgent owner)
    {
        this.owner = owner;
        attackService = new AttackService(
            () => owner.RuntimeStats.Damage,
            () => owner.RuntimeStats.AttackCooldown,
            () => owner.AttackOrigin.position);
        enabled = false;
    }

    public void Activate(SquadCombatCoordinator squadCombatCoordinator)
    {
        this.squadCombatCoordinator = squadCombatCoordinator;
        attackService?.ResetCooldown();
        enabled = true;
    }

    public void Deactivate()
    {
        enabled = false;
        currentTarget = null;
        squadCombatCoordinator = null;
    }

    private void Update()
    {
        if (owner == null || !owner.IsAlive || squadCombatCoordinator == null)
            return;

        currentTarget = squadCombatCoordinator.GetClosestLivingAlly(owner.transform.position) as ICombatTarget;
        if (currentTarget == null || !currentTarget.IsAlive)
            return;

        Vector3 direction = currentTarget.transform.position - owner.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        float sqrRange = owner.RuntimeStats.AttackRange * owner.RuntimeStats.AttackRange;
        if (direction.sqrMagnitude > sqrRange)
            return;

        if (attackService.Tick(Time.deltaTime, currentTarget))
            owner.SpawnProjectileVisual(currentTarget.transform);
    }
}
