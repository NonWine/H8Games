using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class StartButtleView : MonoBehaviour
{
    [Inject] private SquadCombatStateController _squadCombatStateController;
    [SerializeField] private Button startButton;

    private void OnEnable()
    {
        startButton.onClick.AddListener(StartFlow);
    }



    private void OnDisable()
    {
        startButton.onClick.RemoveListener(StartFlow);
    }

    public void StartFlow()
    {
        _squadCombatStateController.StartFlow();
    }
}