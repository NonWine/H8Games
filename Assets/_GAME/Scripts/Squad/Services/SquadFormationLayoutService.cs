using System.Collections.Generic;
using UnityEngine;

public sealed class SquadFormationLayoutService
{
    private readonly SquadFollowSettings settings;

    public SquadFormationLayoutService(SquadFollowSettings settings)
    {
        this.settings = settings;
    }

    public List<Vector3> CalculateLocalOffsets(int slotCount)
    {
        List<Vector3> offsets = new(slotCount);
        int columns = settings.Columns;

        for (int row = 0, slotIndex = 0; slotIndex < slotCount; row++)
        {
            int itemsInRow = Mathf.Min(columns, slotCount - slotIndex);
            float rowWidth = (itemsInRow - 1) * settings.SpacingX;
            float startX = -rowWidth * 0.5f;
            float z = -(row + 1) * settings.SpacingZ;

            for (int column = 0; column < itemsInRow; column++, slotIndex++)
            {
                float x = startX + column * settings.SpacingX;
                offsets.Add(new Vector3(x, 0f, z));
            }
        }

        return offsets;
    }
}
