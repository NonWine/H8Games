using System;

public interface IHealthModule
{
    float MaxHealth { get; }
    float CurrentHealth { get; }

    event Action Died;
    event Action<float, float> HealthChanged;

    void ApplyDamage(float amount);
    void RestoreFull();
}
