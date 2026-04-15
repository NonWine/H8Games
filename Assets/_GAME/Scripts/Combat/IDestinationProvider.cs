using UnityEngine;

public interface IDestinationProvider
{
    bool HasDestination { get; }
    Vector3 Destination { get; }
}