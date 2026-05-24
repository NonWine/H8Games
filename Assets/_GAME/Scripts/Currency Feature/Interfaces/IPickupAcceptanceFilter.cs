public interface IPickupAcceptanceFilter
{
    bool CanAccept(string pickupId, int amount);
}
