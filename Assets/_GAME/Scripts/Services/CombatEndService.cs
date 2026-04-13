using System;

public sealed class CombatEndService
{
    public event Action<CombatResult> Finished;

    public CombatResult Result { get; private set; }
    public bool IsFinished => Result != CombatResult.None;

    public void FinishWin()
    {
        Finish(CombatResult.Win);
    }

    public void FinishLose()
    {
        Finish(CombatResult.Lose);
    }

    private void Finish(CombatResult result)
    {
        if (IsFinished)
            return;

        Result = result;
        Finished?.Invoke(Result);
    }
}
