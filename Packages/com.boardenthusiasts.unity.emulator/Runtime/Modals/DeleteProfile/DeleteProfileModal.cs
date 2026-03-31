using System;
using System.Linq;

using BE.Emulator.Actions;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Rahmen.Logging;

using Zenject;

namespace BE.Emulator.Modals.DeleteProfile
{
/// <summary>
/// Controller for the delete-profile modal.
/// </summary>
internal sealed class DeleteProfileModal
    : DisplayableController<DeleteProfileModalModel, DeleteProfileModalView, DeleteProfileModalViewModel>, IEmulatorModal, IViewActionHandler
{
    /// <summary>
    /// The logical display name for the delete-profile modal.
    /// </summary>
    public const string DisplayName = "delete-profile";

    /// <inheritdoc />
    public string Name => DisplayName;

    private readonly DeleteProfileModalModel _model;
    private readonly DeleteProfileModalView _view;
    private readonly IEmulatorModel _emulatorModel;
    private string _selectedProfileId;

    /// <summary>
    /// Creates the delete-profile modal controller.
    /// </summary>
    public DeleteProfileModal(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] DeleteProfileModalModel model,
        [NotNull] DeleteProfileModalView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter,
        [InjectOptional] IEmulatorModel emulatorModel = null)
        : base(loggerFactory, model, view, displayActionRouter)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _emulatorModel = emulatorModel;
    }

    /// <inheritdoc />
    public void HandleRoutedViewAction(object sender, IViewAction action)
    {
        switch(action)
        {
            case OpenDeleteProfileViewAction openAction:
                _selectedProfileId = openAction.SelectedProfileId;
                RefreshSelectedProfile();
                return;

            case DeleteProfileViewAction deleteAction:
                _view.BeginDeleteFlow(() =>
                {
                    if(_emulatorModel?.DeleteProfile(deleteAction.SelectedProfileId) != true)
                    {
                        _view.EndDeleteFlow();
                        return;
                    }

                    RouteViewAction(this, new CloseDeleteProfileViewAction());
                    RouteViewAction(this, new CloseProfileSettingsViewAction());
                });
                return;
        }
    }

    private void RefreshSelectedProfile()
    {
        var selectedProfile = _emulatorModel?.CurrentData?.Profiles?.FirstOrDefault(profile =>
            string.Equals(profile?.PlayerId, _selectedProfileId, StringComparison.Ordinal));
        _view.SetModalState(_model.CreateState(selectedProfile));
    }
}
}
