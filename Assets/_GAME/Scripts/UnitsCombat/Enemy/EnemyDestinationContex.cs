using UnityEngine;

public class EnemyDestinationContex : IDestinationProvider
{
    public bool HasDestination { get; private set; }
    public Vector3 Destination { get; private set; }

    public void Set(Vector3 destination)
    {
        Destination = destination;
        HasDestination = true;
    }

    public void Clear()
    {
        HasDestination = false;
    }
}