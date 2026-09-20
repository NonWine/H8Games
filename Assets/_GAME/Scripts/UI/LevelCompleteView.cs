using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class LevelCompleteView : MonoBehaviour
{
    [SerializeField] private GameObject root;

    [Inject] private SignalBus signalBus;

    private CancellationTokenSource hideCts;

    [SerializeField] private float displayDurationSeconds = 1.5f;

    private void Awake()
    {
        signalBus.Subscribe<LevelCompletedSignal>(Show);
        signalBus.Subscribe<StartButtleSignal>(Hide);
    }

    private void OnDestroy()
    {
        signalBus.Unsubscribe<LevelCompletedSignal>(Show);
        signalBus.Unsubscribe<StartButtleSignal>(Hide);
        hideCts?.Cancel();
        hideCts?.Dispose();
    }

    private void Show()
    {
        root.SetActive(true);

        hideCts?.Cancel();
        hideCts?.Dispose();
        hideCts = new CancellationTokenSource();
        AutoHideAsync(hideCts.Token).Forget();
    }

    private void Hide()
    {
        hideCts?.Cancel();
        root.SetActive(false);
    }

    private async UniTaskVoid AutoHideAsync(CancellationToken cancellationToken)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(displayDurationSeconds), cancellationToken: cancellationToken);
        root.SetActive(false);
    }
}
