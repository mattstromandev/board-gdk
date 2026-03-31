using System;
using System.Collections.Generic;
using System.Linq;

using BE.Emulator.Actions;
using BE.Emulator.Data;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;
using BE.Emulator.Services;

using JetBrains.Annotations;

using Rahmen.Logging;

using Zenject;

namespace BE.Emulator
{
/// <summary>
/// Root shell controller for the Board OS emulator UI.
/// </summary>
internal sealed class Emulator
    : DisplayableController<EmulatorModel, EmulatorView, EmulatorViewModel>, IViewActionHandler
{
    private readonly IEmulatorModel _model;
    private readonly EmulatorPanelHost _panelHost;
    private readonly IReadOnlyList<IEmulatorDisplay> _displays;
    private readonly IEmulatorDisplayStateStore _displayStateStore;
    private readonly IDisplayHostResolver _displayHostResolver;
    private readonly IEmulatorDisplayStateSynchronizer _displayStateSynchronizer;
    private bool _haveAttachedDisplays;
    private bool _isSafeSpaceVisible;

    /// <summary>
    /// Creates the emulator shell controller.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="model">The shell model.</param>
    /// <param name="view">The shell view.</param>
    /// <param name="displayActionRouter">The action router used to route view actions.</param>
    /// <param name="panelHost">The runtime panel host that owns the shell document.</param>
    /// <param name="displays">The displays that should be attached into the shell.</param>
    /// <param name="displayStateStore">The store that tracks display visibility state.</param>
    /// <param name="displayHostResolver">The resolver used to map displays to host elements.</param>
    /// <param name="displayStateSynchronizer">The synchronizer used to reflect display state into the shell view.</param>
    public Emulator(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] EmulatorModel model,
        [NotNull] EmulatorView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter,
        [NotNull] EmulatorPanelHost panelHost,
        [NotNull] IEnumerable<IEmulatorDisplay> displays,
        [NotNull] IEmulatorDisplayStateStore displayStateStore,
        [NotNull] IDisplayHostResolver displayHostResolver,
        [NotNull] IEmulatorDisplayStateSynchronizer displayStateSynchronizer)
        : base(loggerFactory, model, view, displayActionRouter)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _panelHost = panelHost ?? throw new ArgumentNullException(nameof(panelHost));
        _displays = displays is IReadOnlyList<IEmulatorDisplay> list ? list : new List<IEmulatorDisplay>(displays ?? throw new ArgumentNullException(nameof(displays)));
        _displayStateStore = displayStateStore ?? throw new ArgumentNullException(nameof(displayStateStore));
        _displayHostResolver = displayHostResolver ?? throw new ArgumentNullException(nameof(displayHostResolver));
        _displayStateSynchronizer = displayStateSynchronizer ?? throw new ArgumentNullException(nameof(displayStateSynchronizer));
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        Attach(_panelHost.Root, clearHost: true, hideAfterAttach: false);
        Show();
        RefreshFromModel();
        TryAttachDisplays();

        EmulatorExternalViewActionBridge.ViewActionRequested += OnExternalViewActionRequested;
        _model.Changed += OnModelChanged;
        _model.ActiveProfileChanged += OnModelChanged;
        _model.PlayersChanged += OnModelChanged;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        EmulatorExternalViewActionBridge.ViewActionRequested -= OnExternalViewActionRequested;
        _model.Changed -= OnModelChanged;
        _model.ActiveProfileChanged -= OnModelChanged;
        _model.PlayersChanged -= OnModelChanged;

        foreach(IEmulatorDisplay display in _displays)
        {
            if(display.IsAttached)
            {
                display.Detach();
            }
        }

        Detach();
        base.Dispose();
    }

    private void OnModelChanged(object sender, EventArgs eventArgs)
    {
        RefreshFromModel();
    }

    private void RefreshFromModel()
    {
        EmulatorMockData currentData = _model.CurrentData;
        EmulatorProfileData activeProfile = null;
        string activeProfileId = currentData?.Session?.ActiveProfileId;

        if(currentData?.Profiles != null)
        {
            foreach(EmulatorProfileData profile in currentData.Profiles)
            {
                activeProfile ??= profile;
                if(string.Equals(profile?.PlayerId, activeProfileId, StringComparison.Ordinal))
                {
                    activeProfile = profile;
                    break;
                }
            }
        }

        View.SetActiveProfile(activeProfile);
        View.SetProfileSwitcherVisible(currentData?.Application?.IsProfileSwitcherVisible ?? false);
        View.SetSafeSpaceVisible(_isSafeSpaceVisible);
        _displayStateSynchronizer.Refresh();
    }

    private void OnExternalViewActionRequested(object sender, ViewActionEventArgs eventArgs)
    {
        if(eventArgs?.Action == null)
        {
            return;
        }

        RouteViewAction(sender, eventArgs.Action);
    }

    private void TryAttachDisplays()
    {
        if(_haveAttachedDisplays)
        {
            return;
        }

        foreach(IEmulatorDisplay display in _displays)
        {
            DisplayHostResolution resolution = _displayHostResolver.Resolve(display);
            display.Attach(resolution.Host, resolution.ClearHostOnAttach);

            if(_displayStateStore.IsVisible(display.GetType()))
            {
                display.Show();
            }
        }

        _haveAttachedDisplays = true;
        _displayStateSynchronizer.Refresh();
    }

    /// <inheritdoc />
    public void HandleRoutedViewAction(object sender, IViewAction action)
    {
        switch(action)
        {
            case ProfileSelectedViewAction profileSelectedAction:
                if(_model.SetActiveProfile(profileSelectedAction.SelectedProfileId) == false)
                {
                    return;
                }

                RouteViewAction(this, new CloseProfileSwitcherViewAction());
                return;

            case SetSafeSpaceVisibilityViewAction safeSpaceAction:
                _isSafeSpaceVisible = safeSpaceAction.IsVisible;
                View.SetSafeSpaceVisible(_isSafeSpaceVisible);
                return;
        }
    }
}
}
