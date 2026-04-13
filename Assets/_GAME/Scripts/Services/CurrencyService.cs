using System;
using UnityEngine;

public class CurrencyService
{
    public event Action<int> AmountChanged;

    public int Amount { get; private set; }

    public void Set(int amount)
    {
        Amount = Mathf.Max(0, amount);
        AmountChanged?.Invoke(Amount);
    }

    public void Add(int amount)
    {
        if (amount <= 0)
            return;

        Amount += amount;
        AmountChanged?.Invoke(Amount);
    }

    public bool CanSpend(int amount)
    {
        return amount >= 0 && Amount >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (!CanSpend(amount))
            return false;

        Amount -= amount;
        AmountChanged?.Invoke(Amount);
        return true;
    }
}
