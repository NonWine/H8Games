using System.Collections.Generic;

public class NullPickupStackImpactFeedback : IPickupStackImpactFeedback
{
    public void PlayLanding(IReadOnlyList<PickupItemController> stack, PickupItemController landed) { }
}
