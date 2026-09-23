using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FloatingTextPool : MonoBehaviour
{
    [SerializeField] private FloatingTextView prefab;

    private readonly Stack<FloatingTextView> available = new Stack<FloatingTextView>();

    public void Prepare(int size)
    {
        if (available.Count > 0)
        {
            return;
        }

        for (int i = 0; i < size; i++)
        {
            FloatingTextView view = Instantiate(prefab, transform);
            view.Prepare(Release);
            available.Push(view);
        }
    }

    public bool TryTake(out FloatingTextView view)
    {
        if (available.Count == 0)
        {
            view = null;
            return false;
        }

        view = available.Pop();
        return true;
    }

    private void Release(FloatingTextView view)
    {
        available.Push(view);
    }
}
