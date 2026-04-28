using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Unit Config", fileName = "UnitConfig", order = 0)]
public class UnitConfig : ScriptableObject
{
    [SerializeField] private UnitStats unitStats = new();
    [SerializeField] private TargetingData targetingData = new();

    public UnitStats AuthoringStats => unitStats;
    
    public TargetingData TargetingData => targetingData;

    public UnitStats CreateRuntimeStats()
    {
        return unitStats.Clone();
    }
    
    public TargetingData CreateTargetingData()
    {
        return targetingData;
    }
    
}
