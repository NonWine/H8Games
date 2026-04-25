using System;
using UnityEngine;

public class UnitHealthHandler
{
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public bool IsAlive { get; private set; }
    public event Action<float, float> HealthChanged;
    public event Action Died;

    public UnitHealthHandler(float maxHealth)
    {
        MaxHealth = Mathf.Max(1f, maxHealth);
        CurrentHealth = MaxHealth;
        IsAlive = true;
    }



    public void ApplyDamage(float amount)
    {
        
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            IsAlive = false;
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
        IsAlive = true;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}
