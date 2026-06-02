using System;
using UnityEngine;

public class UnitHealthHandler : IHealthModule, IResetModule
{
    private readonly IAliveState aliveState;

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public event Action<float, float> HealthChanged;
    public event Action Died;

    public UnitHealthHandler(float maxHealth, IAliveState aliveState)
    {
        this.aliveState = aliveState;
        MaxHealth = Mathf.Max(1f, maxHealth);
        CurrentHealth = MaxHealth;
        this.aliveState.IsAlive = true;
    }



    public void ApplyDamage(float amount)
    {
        
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            aliveState.IsAlive = false;
            Died?.Invoke();
        }
    }

    public void IncreaseMaxHealth(float amount, bool healByAddedAmount)
    {
        if (amount <= 0f)
            return;

        MaxHealth += amount;
        CurrentHealth = healByAddedAmount
            ? Mathf.Min(MaxHealth, CurrentHealth + amount)
            : Mathf.Min(CurrentHealth, MaxHealth);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void RestoreFull()
    {
        CurrentHealth = MaxHealth;
        aliveState.IsAlive = true;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // Called by CombatUnitModules.ResetModules() on pool respawn so a reused
    // instance starts at full health and alive instead of the dead state it
    // was despawned in.
    public void Reset() => RestoreFull();
}
