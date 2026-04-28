using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitCombatDefinition", menuName = "Configs/UnitCombatDefinition")]
public class UnitCombatDefinition : ScriptableObject
{
    [SerializeField] private string unitID;
    [SerializeField] private BaseCombatUnitView prefab;
    [SerializeField] private int initialPoolSize = 5;

    public string UnitID => unitID;
    public BaseCombatUnitView Prefab => prefab;
    public int InitialPoolSize => initialPoolSize;
    
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(unitID))
        {
            RegenerateID();
        }
    }

    public void RegenerateID()
    {
        unitID = Guid.NewGuid().ToString();
    }
}
