public class NullPickupAcceptanceFilter : IPickupAcceptanceFilter
{
    public bool CanAccept(string pickupId, int amount) => true;
}
