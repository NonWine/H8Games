using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class StartButtonInteractorView : MonoBehaviour
{
    [SerializeField] private StartButtleView startButtleView;
    [Inject] private SignalBus signalBus;
    [SerializeField] private Transform root;
    private BoxCollider boxCollider;
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        signalBus.Subscribe<LoadNextLevelSignal>(ShowRoot);
        signalBus.Subscribe<GameIdleStateSignal>(ShowRoot);
        signalBus.Subscribe<StartButtleSignal>(HideRoot);
    }

    private void OnDestroy()
    {
        signalBus.Unsubscribe<LoadNextLevelSignal>(ShowRoot);
        signalBus.Unsubscribe<GameIdleStateSignal>(ShowRoot);
        signalBus.Unsubscribe<StartButtleSignal>(HideRoot);

    }

    private void HideRoot()
    {
        boxCollider.enabled = false;
        root.transform.DOScale(0f,0.2f).SetEase(Ease.Linear)
            .OnComplete(() => root.gameObject.SetActive(false));
        startButtleView.transform.DOScale(0f, 0.15f).SetEase(Ease.Linear);

    }

    private void ShowRoot()
    {
        boxCollider.enabled = true;
        root.gameObject.SetActive(true);
        root.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerView playerView))
        {
            transform?.DOKill();
            startButtleView.DOKill();

            transform.DOScale(1f, 0.15f).SetEase(Ease.Linear)
                .OnComplete(() => startButtleView.gameObject.SetActive(false));
            startButtleView.transform.DOScale(0f, 0.15f).SetEase(Ease.Linear);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.TryGetComponent(out PlayerView playerView))
        {
            transform?.DOKill();
            startButtleView.DOKill();
            startButtleView.gameObject.SetActive(true);
            startButtleView.transform.localScale = Vector3.zero;
        
            transform.DOScale(1.25f, 0.25f).SetEase(Ease.OutBack);
            startButtleView.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);   
        }
    }

}