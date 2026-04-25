using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyDeathModule : UnitDeathModule
{
    private readonly CurrencyService currencyService;
    private readonly int reward;
   
    public EnemyDeathModule(GameObject ownerObject, int disableDelayMs = 5000) : base(ownerObject, disableDelayMs)
    {
    }
    
    public override async UniTask HandleDeathAsync(Action beforeDisable = null)
    {
        await base.HandleDeathAsync(beforeDisable);
        currencyService.Add(reward);

    }

}
