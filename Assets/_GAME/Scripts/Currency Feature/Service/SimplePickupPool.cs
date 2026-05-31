using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public sealed class SimplePickupPool : IPickupPool
{
    private readonly Dictionary<GameObject, Stack<PickupItemView>> stacks   = new();
    private readonly Dictionary<PickupItemView, GameObject>        prefabOf = new();

    public async UniTask<PickupItemView> RentAsync(GameObject prefab, CancellationToken ct)
    {
        await UniTask.CompletedTask;

        if (!stacks.TryGetValue(prefab, out var stack))
        {
            stack          = new Stack<PickupItemView>();
            stacks[prefab] = stack;
        }

        PickupItemView view;

        if (stack.Count > 0)
        {
            view = stack.Pop();
        }
        else
        {
            var go         = Object.Instantiate(prefab);
            view           = go.GetComponent<PickupItemView>();
            prefabOf[view] = prefab;
        }

        view.gameObject.SetActive(true);
        view.Rent();

        return view;
    }

    public void Return(PickupItemView view)
    {
        view.Cleanup();
        view.gameObject.SetActive(false);

        if (!prefabOf.TryGetValue(view, out var prefab))
            return;

        if (!stacks.TryGetValue(prefab, out var stack))
        {
            stack          = new Stack<PickupItemView>();
            stacks[prefab] = stack;
        }

        stack.Push(view);
    }
}
