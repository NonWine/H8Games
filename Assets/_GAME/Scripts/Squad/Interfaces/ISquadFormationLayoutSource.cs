using System;
using System.Collections.Generic;

// Narrow read side of the formation: everything a presenter needs to draw the
// slots, and nothing that could let it move a soldier.
public interface ISquadFormationLayoutSource
{
    event Action FormationChanged;

    IReadOnlyList<FormationSlot> Slots { get; }
}
