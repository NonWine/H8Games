using System;

public sealed class RewardOnDeathService : IDisposable
{
    private readonly UnitHealthHandler _unitHealthHandler;
    private readonly CurrencyService currencyService;
    private readonly int reward;

    public RewardOnDeathService(UnitHealthHandler unitHealthHandler, CurrencyService currencyService, int reward)
    {
        this._unitHealthHandler = unitHealthHandler;
        this.currencyService = currencyService;
        this.reward = reward;
        this._unitHealthHandler.Died += HandleDeath;
    }

    public void Dispose()
    {
        _unitHealthHandler.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        currencyService?.Add(reward);
    }
}
