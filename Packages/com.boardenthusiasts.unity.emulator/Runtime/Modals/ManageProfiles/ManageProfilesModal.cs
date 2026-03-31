using System;

using BE.Emulator.Framework;
using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Rahmen.Logging;

using Zenject;

namespace BE.Emulator.Modals.ManageProfiles
{
/// <summary>
/// Controller for the manage profiles modal.
/// </summary>
internal sealed class ManageProfilesModal
    : DisplayableController<ManageProfilesModalModel, ManageProfilesModalView, ManageProfilesModalViewModel>, IEmulatorModal
{
    /// <summary>
    /// The logical display name for the manage profiles modal.
    /// </summary>
    public const string DisplayName = "manage-profiles";

    /// <inheritdoc />
    public string Name => DisplayName;

    private readonly ManageProfilesModalModel _model;
    private readonly IEmulatorModel _emulatorModel;

    /// <summary>
    /// Creates the manage profiles modal controller.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="model">The backing model used by the controller.</param>
    /// <param name="view">The view controlled by this controller.</param>
    /// <param name="displayActionRouter">The router that should handle published view actions.</param>
    /// <param name="emulatorModel">The optional emulator model used to populate profile data.</param>
    public ManageProfilesModal(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] ManageProfilesModalModel model,
        [NotNull] ManageProfilesModalView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter,
        [InjectOptional] IEmulatorModel emulatorModel = null)
        : base(loggerFactory, model, view, displayActionRouter)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _emulatorModel = emulatorModel;
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        RefreshFromModel();

#if UNITY_EDITOR
        if(_emulatorModel != null)
        {
            _emulatorModel.Changed += OnEmulatorModelChanged;
        }
#endif
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

    private void OnEmulatorModelChanged(object sender, EventArgs eventArgs)
    {
        RefreshFromModel();
    }

    private void RefreshFromModel()
    {
        _model.SetProfileItems(_emulatorModel?.CurrentData?.Profiles?.ConvertAll(profile => new ManageProfilesProfileItemViewModel
        {
            PlayerId = profile?.PlayerId ?? string.Empty,
            DisplayName = profile?.DisplayName ?? string.Empty,
            AvatarImage = new UnityEngine.UIElements.StyleBackground(profile?.Avatar),
            AvatarBackgroundColor = new UnityEngine.UIElements.StyleColor(profile?.AvatarBackgroundColor ?? default)
        }));
        View.SetProfileItems(_model.ProfileItems);
    }
}
}
