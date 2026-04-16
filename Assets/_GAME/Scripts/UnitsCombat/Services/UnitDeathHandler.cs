using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class UnitDeathHandler
{
    private readonly GameObject ownerObject;
    private readonly int disableDelayMs;

    public UnitDeathHandler(GameObject ownerObject, int disableDelayMs = 5000)
    {
        this.ownerObject = ownerObject;
        this.disableDelayMs = disableDelayMs;
    }

    public async UniTask HandleDeathAsync(Action beforeDisable = null)
    {
        beforeDisable?.Invoke();
        await UniTask.Delay(disableDelayMs);
        if(ownerObject != null) ownerObject.transform.DOMoveY(-3f, 5f).OnComplete(() => ownerObject.gameObject.SetActive(false));
    }
}