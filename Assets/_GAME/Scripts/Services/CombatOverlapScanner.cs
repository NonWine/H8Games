using System;

public class GamePhaseService
{
    public event Action<GamePhase> Changed;

    public GamePhaseService(GamePhase initialPhase)
    {
        CurrentPhase = initialPhase;
    }

    public GamePhase CurrentPhase { get; private set; }
    public bool IsPreparation => CurrentPhase == GamePhase.Preparation;
    public bool IsBattle => CurrentPhase == GamePhase.Battle;

    public void EnterPreparation()
    {
        SetPhase(GamePhase.Preparation);
    }

    public void EnterBattle()
    {
        SetPhase(GamePhase.Battle);
    }

    private void SetPhase(GamePhase phase)
    {
        if (CurrentPhase == phase)
            return;

        CurrentPhase = phase;
        Changed?.Invoke(CurrentPhase);
    }
}