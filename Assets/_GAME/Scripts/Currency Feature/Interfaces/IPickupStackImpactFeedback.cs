using System.Collections.Generic;

public interface IPickupStackImpactFeedback
{
    void PlayLanding(IReadOnlyList<PickupItemController> stack, PickupItemController landed);
}
