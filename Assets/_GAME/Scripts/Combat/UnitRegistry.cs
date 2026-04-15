using System;
using System.Collections.Generic;

public class UnitRegistry<T> where T : class
{
    private readonly List<T> items = new();
    private readonly Func<T, bool> isValid;

    public UnitRegistry(Func<T, bool> isValid)
    {
        this.isValid = isValid;
    }

    public IReadOnlyList<T> Items => items;

    public int Count
    {
        get
        {
            PruneInvalid();
            return items.Count;
        }
    }

    public bool Register(T item)
    {
        if (item == null || items.Contains(item))
            return false;

        items.Add(item);
        return true;
    }

    public void Unregister(T item)
    {
        if (item == null)
            return;

        items.Remove(item);
    }

    public void PruneInvalid()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (isValid(items[i]))
                continue;

            items.RemoveAt(i);
        }
    }
}