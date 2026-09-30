using System.Collections.Generic;
using UnityEngine;

public class PickupStackImpactFeedback : IPickupStackImpactFeedback
{
    private readonly IPickupCarryStackSpring spring;

    public PickupStackImpactFeedback(IPickupCarryStackSpring spring)
    {
        this.spring = spring;
    }

    public void PlayLanding(IReadOnlyList<PickupItemController> stack, PickupItemController landed)
    {
        var settings = landed.StackImpact;

        if (settings == null || !settings.Enabled)
            return;

        var landedIndex = IndexOf(stack, landed);

        if (landedIndex < 0)
        {
            landed.PlayStackImpact(settings, 1f, 1f, 0f);
            return;
        }

        var columnCompression = 0f;

        for (var i = 0; i < stack.Count; i++)
        {
            var distance    = Mathf.Abs(i - landedIndex);
            var compression = distance > settings.MaxAffectedNeighbours ? 0f : Mathf.Pow(settings.Falloff, distance);

            columnCompression += compression;

            if (compression <= 0f)
                continue;

            stack[i].PlayStackImpact(
                settings,
                compression,
                columnCompression,
                Mathf.Max(0, landedIndex - i) * settings.PropagationDelay);
        }

        if (settings.ColumnKick > 0f)
            spring.AddImpulse(RandomHorizontal() * settings.ColumnKick, landedIndex);
    }

    private static Vector3 RandomHorizontal()
    {
        var angle = Random.value * Mathf.PI * 2f;

        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    private static int IndexOf(IReadOnlyList<PickupItemController> stack, PickupItemController target)
    {
        for (var i = 0; i < stack.Count; i++)
        {
            if (stack[i] == target)
                return i;
        }

        return -1;
    }
}
