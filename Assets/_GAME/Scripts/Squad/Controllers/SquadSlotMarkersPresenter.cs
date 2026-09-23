using System;
using Zenject;

public class SquadSlotMarkersPresenter : IInitializable, IDisposable
{
    private readonly ISquadFormationLayoutSource layoutSource;
    private readonly SquadSlotMarkersView view;
    private readonly SquadRootView squadRootView;

    public SquadSlotMarkersPresenter(
        ISquadFormationLayoutSource layoutSource,
        SquadSlotMarkersView view,
        SquadRootView squadRootView)
    {
        this.layoutSource = layoutSource;
        this.view = view;
        this.squadRootView = squadRootView;
    }

    // The markers are anchored to the home pose once and never touched again: the
    // slot offsets they draw are the same local offsets the root uses, so pinning
    // them here shows the parking spots the squad regroups into instead of
    // dragging the whole pad along on the march.
    //
    // The formation itself is built inside SquadFormationController's constructor,
    // long before anything can subscribe, so the first draw is pulled here rather
    // than waited for.
    public void Initialize()
    {
        view.AnchorTo(squadRootView.HomePosition, squadRootView.HomeRotation);
        layoutSource.FormationChanged += Redraw;
        Redraw();
    }

    public void Dispose()
    {
        layoutSource.FormationChanged -= Redraw;
    }

    private void Redraw()
    {
        view.ApplyLayout(layoutSource.Slots);
    }
}
