using System;
using System.Linq;

using BE.Emulator.Actions;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Rahmen.Logging;

using Zenject;

namespace BE.Emulator.Modals.ProfileSettings
{
/// <summary>
/// Controller for the profile settings modal.
/// </summary>
internal sealed class ProfileSettingsModal
    : DisplayableController<ProfileSettingsModalModel, ProfileSettingsModalView, ProfileSettingsModalViewModel>, IEmulatorModal, IViewActionHandler
{
    /// <summary>
    /// The logical display name for the profile settings modal.
    /// </summary>
    public const string DisplayName = "profile-settings";

    /// <inheritdoc />
    public string Name => DisplayName;

    private readonly ProfileSettingsModalModel _model;
    private readonly ProfileSettingsModalView _view;
    private readonly IEmulatorModel _emulatorModel;
    private string _selectedProfileId;

    /// <summary>
    /// Creates the profile settings modal controller.
    /// </summary>
    public ProfileSettingsModal(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] ProfileSettingsModalModel model,
        [NotNull] ProfileSettingsModalView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter,
        [InjectOptional] IEmulatorModel emulatorModel = null)
        : base(loggerFactory, model, view, displayActionRouter)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _emulatorModel = emulatorModel;
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

#if UNITY_EDITOR
        if(_emulatorModel != null)
        {
            _emulatorModel.Changed += OnEmulatorModelChanged;
        }
#endif

        RefreshSelectedProfile();
    }

    /// <inheritdoc />
    public override void Dispose()
    {
#if UNITY_EDITOR
        if(_emulatorModel != null)
        {
            _emulatorModel.Changed -= OnEmulatorModelChanged;
        }
#endif

        base.Dispose();
    }

    /// <inheritdoc />
    public void HandleRoutedViewAction(object sender, IViewAction action)
    {
        if(action is not OpenProfileSettingsViewAction openAction)
        {
            return;
        }

        _selectedProfileId = openAction.SelectedProfileId;
        RefreshSelectedProfile();
    }

    private void OnEmulatorModelChanged(object sender, EventArgs eventArgs)
    {
        RefreshSelectedProfile();
    }

    private void RefreshSelectedProfile()
    {
        var selectedProfile = _emulatorModel?.CurrentData?.Profiles?.FirstOrDefault(profile =>
            string.Equals(profile?.PlayerId, _selectedProfileId, StringComparison.Ordinal));
        _view.SetProfileState(_model.CreateState(selectedProfile));
    }
}
}
