using System;
using Zenject;

public class SquadLayoutFullFeedbackPresenter : IInitializable, IDisposable
{
    private readonly ISquadAssemblyReader assembly;
    private readonly SquadLayoutFullFeedbackView view;

    public SquadLayoutFullFeedbackPresenter(ISquadAssemblyReader assembly, SquadLayoutFullFeedbackView view)
    {
        this.assembly = assembly;
        this.view = view;
    }

    public void Initialize()
    {
        assembly.AssemblyChanged += Refresh;
        view.SetFull(assembly.IsAssembled, true);
    }

    public void Dispose()
    {
        assembly.AssemblyChanged -= Refresh;
    }

    private void Refresh()
    {
        view.SetFull(assembly.IsAssembled, false);
    }
}
