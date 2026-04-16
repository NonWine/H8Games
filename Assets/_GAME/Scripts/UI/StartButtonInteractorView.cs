using System;
using DG.Tweening;
using UnityEngine;

public class StartButtonInteractorView : MonoBehaviour
{
    [SerializeField] private StartButtleView startButtleView;
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
        
        transform?.DOKill();
        startButtleView.DOKill();
        startButtleView.gameObject.SetActive(true);
        startButtleView.transform.localScale = Vector3.zero;
        
        transform.DOScale(1.25f, 0.25f).SetEase(Ease.OutBack);
        startButtleView.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack);
    }

}