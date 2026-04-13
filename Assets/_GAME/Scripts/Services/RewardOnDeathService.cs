using System;

public sealed class RewardOnDeathService : IDisposable
{
    private readonly HealthService healthService;
    private readonly CurrencyService currencyService;
    private readonly int reward;

    public RewardOnDeathService(HealthService healthService, CurrencyService currencyService, int reward)
    {
        this.healthService = healthService;
        this.currencyService = currencyService;
        this.reward = reward;
        this.healthService.Died += HandleDeath;
    }

    public void Dispose()
    {
        healthService.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        currencyService?.Add(reward);
    }
}
