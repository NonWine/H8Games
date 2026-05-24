public interface IPickupMagnetProvider
{
    bool TryGetMagnet(out PickupMagnet magnet);
}
