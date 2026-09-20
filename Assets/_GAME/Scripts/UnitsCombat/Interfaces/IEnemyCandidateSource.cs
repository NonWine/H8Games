using System.Collections.Generic;

// The pool of enemies an ally may engage. Deliberately narrow: a target provider
// only needs to know who is out there, never how the level is assembled or which
// group the squad happens to be marching at.
public interface IEnemyCandidateSource
{
    IReadOnlyList<ITargetSelectionCandidate> Candidates { get; }
}
