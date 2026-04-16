using System;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitCombatDefinition", menuName = "Configs/UnitCombatDefinition")]
public class UnitCombatDefinition : ScriptableObject
{
    private string unitID;
    [SerializeField] private BaseCombatUnitView prefab;
    
    public string UnitID => unitID;
    public BaseCombatUnitView Prefab => prefab;
    
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
