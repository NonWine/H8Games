using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class StartButtleView : MonoBehaviour
{
    [Inject] private CombatStateController combatStateController;
    [SerializeField] private Button startButton;

    private void Awake()
    {
        startButton.onClick.AddListener(StartFlow);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartFlow);
    }

    public void StartFlow()
    {
        combatStateController.StartFlow();
    }
}