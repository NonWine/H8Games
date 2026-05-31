using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class UnitDeathModule : IDeathModule
{
    private readonly GameObject ownerObject;
    private readonly int disableDelayMs;

    public UnitDeathModule(GameObject ownerObject, int disableDelayMs = 5000)
    {
        this.ownerObject = ownerObject;
        this.disableDelayMs = disableDelayMs;
    }

    public virtual async UniTask HandleDeathAsync(Action beforeDisable = null)
    {
        beforeDisable?.Invoke();
       // await UniTask.Delay(disableDelayMs);
        //if(ownerObject != null) ownerObject.transform.DOMoveY(-3f, 5f).OnComplete(() => ownerObject.gameObject.SetActive(false));
    }
}
