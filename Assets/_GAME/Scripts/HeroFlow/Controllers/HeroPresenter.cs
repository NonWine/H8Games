using UnityEngine;
using Zenject;

public sealed class HeroPresenter : IInitializable, ITickable, System.IDisposable
{
    private readonly PlayerView heroView;
    private readonly HeroCombatRuntime runtime;
    private readonly IHeroInputReader inputReader;
    private readonly IHeroMover heroMover;
    private readonly TargetSelector targetSelector;
    private readonly AttackService attackService;

    private ICombatTarget currentTarget;

    public HeroPresenter(
        PlayerView heroView,
        HeroCombatRuntime runtime,
        IHeroInputReader inputReader,
        IHeroMover heroMover,
        TargetSelector targetSelector)
    {
        this.heroView = heroView;
        this.runtime = runtime;
        this.inputReader = inputReader;
        this.heroMover = heroMover;
        this.targetSelector = targetSelector;
        attackService = new AttackService(
            () => runtime.RuntimeStats.Combat.Damage,
            () => runtime.RuntimeStats.Combat.AttackCooldown,
            () => heroView.AttackOrigin.position);
    }

    public void Initialize()
    {
        heroView.DamageReceived += HandleDamageReceived;
        runtime.HealthChanged += HandleHealthChanged;
        runtime.Died += HandleDeath;
        heroView.SetHealth(runtime.CurrentHealth, runtime.MaxHealth);
    }

    public void Tick()
    {
        if (!runtime.IsAlive)
            return;

        Vector3 movementDirection = inputReader.ReadMovement();
        if (movementDirection.sqrMagnitude > 0f)
        {
            heroMover.Move(movementDirection, runtime.RuntimeStats.Combat.MoveSpeed, Time.deltaTime);
            heroMover.FaceDirection(movementDirection);
        }

        if (currentTarget == null || !currentTarget.IsAlive || !IsWithinDetection(currentTarget.transform))
        {
            currentTarget = targetSelector.GetClosestEnemy(
                heroView.transform,
                runtime.RuntimeStats.Combat.DetectionRadius,
                heroView.TeamId,
                heroView.DetectionMask);
        }

        if (currentTarget == null)
            return;

        Vector3 aimDirection = currentTarget.transform.position - heroView.transform.position;
        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude > 0.0001f)
            heroMover.FaceDirection(aimDirection.normalized);

        float distance = aimDirection.magnitude;
        if (distance <= runtime.RuntimeStats.Combat.AttackRange &&
            attackService.Tick(Time.deltaTime, currentTarget))
        {
            heroView.SpawnProjectileVisual(currentTarget.transform, runtime.RuntimeStats.Combat.ProjectileSpeed);
        }
    }

    public void Dispose()
    {
        heroView.DamageReceived -= HandleDamageReceived;
        runtime.HealthChanged -= HandleHealthChanged;
        runtime.Died -= HandleDeath;
    }

    private void HandleDamageReceived(float damage, Vector3 sourceWorldPosition)
    {
        runtime.ApplyDamage(damage);
    }

    private bool IsWithinDetection(Transform target)
    {
        Vector3 delta = target.position - heroView.transform.position;
        delta.y = 0f;
        return delta.sqrMagnitude <=
               runtime.RuntimeStats.Combat.DetectionRadius * runtime.RuntimeStats.Combat.DetectionRadius;
    }

    private void HandleHealthChanged(float current, float max)
    {
        heroView.SetHealth(current, max);
    }

    private void HandleDeath()
    {
        heroView.RaiseDeath();
    }
}
