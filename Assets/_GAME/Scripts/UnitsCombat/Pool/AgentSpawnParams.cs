using UnityEngine;

public readonly struct AgentSpawnParams
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly string UnitId;

    public AgentSpawnParams(Vector3 position, Quaternion rotation, string unitId)
    {
        Position = position;
        Rotation = rotation;
        UnitId = unitId;
    }
}
