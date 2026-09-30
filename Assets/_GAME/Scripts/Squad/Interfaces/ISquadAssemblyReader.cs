using System;

public interface ISquadAssemblyReader
{
    event Action AssemblyChanged;

    bool IsAssembled { get; }
}
