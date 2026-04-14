using UnityEngine;
using Zenject;

public sealed class HeroPresenter : IInitializable, ITickable, System.IDisposable
{
    private readonly PlayerView heroView;
    private readonly HeroCombatRuntime runtime;
    private readonly IHeroInputReader inputReader;
    private readonly IHeroMover heroMover;

    public HeroPresenter(
        PlayerView heroView,
        HeroCombatRuntime runtime,
        IHeroInputReader inputReader,
        IHeroMover heroMover)
    {
        this.heroView = heroView;
        this.runtime = runtime;
        this.inputReader = inputReader;
        this.heroMover = heroMover;
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
        heroView.Animator.SetFloat("Speed", movementDirection.magnitude);

        if (movementDirection.sqrMagnitude > 0f)
        {
            heroMover.Move(movementDirection, runtime.RuntimeStats.Combat.MoveSpeed, Time.deltaTime);
            heroMover.FaceDirection(movementDirection);
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

    private void HandleHealthChanged(float current, float max)
    {
        heroView.SetHealth(current, max);
    }

    private void HandleDeath()
    {
        heroView.RaiseDeath();
    }
}
