using System;
using Zenject;

// Owns the single job HeroCombatAgentController used to do on the side: pushing
// health numbers at the bar. Plain C#, so it can be exercised with a fake
// IHealthModule and a fake IHealthView without a scene.
public class HeroHealthPresenter : IInitializable, IDisposable
{
    private readonly IHealthModule health;
    private readonly IHealthView view;

    public HeroHealthPresenter(IHealthModule health, IHealthView view)
    {
        this.health = health;
        this.view = view;
    }

    public void Initialize()
    {
        health.HealthChanged += OnHealthChanged;

        // Seeds the bar with the hero's starting health: nothing has raised
        // HealthChanged yet at this point, so without this push the slider would
        // keep whatever value the prefab was authored with until the first hit.
        OnHealthChanged(health.CurrentHealth, health.MaxHealth);
    }

    public void Dispose()
    {
        health.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float current, float max)
    {
        view.SetHealth(current, max);
    }
}
