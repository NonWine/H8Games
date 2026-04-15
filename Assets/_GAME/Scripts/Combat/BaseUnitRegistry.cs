using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseUnitRegistry<T> where T : class
{
    private readonly UnitRegistry<T> registry;

    protected BaseUnitRegistry(Func<T, bool> isValid)
    {
        registry = new UnitRegistry<T>(isValid);
    }

    public IReadOnlyList<T> Items => registry.Items;
    public int Count => registry.Count;

    public virtual bool Register(T item)
    {
       return registry.Register(item);
    }

    public virtual void Unregister(T item)
    {
        registry.Unregister(item);
    }

    public virtual void PruneInvalid()
    {
        registry.PruneInvalid();
    }

    public bool Contains(T item)
    {
        return registry.Items.Contains(item);
    }
}