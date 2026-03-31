using System;
using System.Linq;

using BE.Emulator.Data;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

using Zenject;

namespace BE.Emulator.Screens.ProfileSwitcher
{
/// <summary>
/// Controller for the profile switcher overlay.
/// </summary>
internal sealed class ProfileSwitcher
    : DisplayableController<ProfileSwitcherModel, ProfileSwitcherView, ProfileSwitcherViewModel>, IEmulatorScreen
{
    /// <summary>
    /// The logical display name for the profile switcher.
    /// </summary>
    public const string DisplayName = "profile-switcher";

    /// <inheritdoc />
    public string Name => DisplayName;

    private readonly ProfileSwitcherModel _model;
    private readonly IEmulatorModel _emulatorModel;

    /// <summary>
    /// Creates the profile switcher controller.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="model">The backing model used by the controller.</param>
    /// <param name="view">The view controlled by this controller.</param>
    /// <param name="displayActionRouter">The router that should handle published view actions.</param>
    /// <param name="emulatorModel">The optional emulator model used to populate profile data.</param>
    public ProfileSwitcher(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] ProfileSwitcherModel model,
        [NotNull] ProfileSwitcherView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter,
        [InjectOptional] [CanBeNull] IEmulatorModel emulatorModel = null)
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
        string activeProfileId = _emulatorModel?.CurrentData?.Session?.ActiveProfileId;
        _model.SetProfileCards(_emulatorModel?.CurrentData.Profiles
            .Select(profile => CreateProfileCard(profile, activeProfileId))
            .ToArray());
        View.SetProfileCards(_model.ProfileCards);
    }

    private static ProfileCardViewModel CreateProfileCard(EmulatorProfileData profile, string activeProfileId)
    {
        return new ProfileCardViewModel
        {
            PlayerId = profile?.PlayerId ?? string.Empty,
            DisplayName = profile?.DisplayName ?? string.Empty,
            AvatarImage = new StyleBackground(profile?.Avatar),
            AvatarBackgroundColor = new StyleColor(profile?.AvatarBackgroundColor ?? default),
            IsActive = string.Equals(profile?.PlayerId, activeProfileId, StringComparison.Ordinal)
        };
    }
}
}
