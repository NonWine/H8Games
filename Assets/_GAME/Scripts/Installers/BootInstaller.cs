using Zenject;

public class BootInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<CurrencyService>().AsSingle();
        Container.Bind<UpgradePriceService>().AsSingle();
    }
}
