using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Unit Config", fileName = "UnitConfig", order = 0)]
public class UnitConfig : ScriptableObject
{
    [SerializeField] private UnitStats unitStats = new();

    public UnitStats AuthoringStats => unitStats;
    public UnitModuleType unitModuleType { get; set; }

    public UnitStats CreateRuntimeStats()
    {
        return unitStats.Clone();
    }
    
}