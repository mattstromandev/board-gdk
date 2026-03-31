using System;
using System.Collections.Generic;

using BE.Emulator.Actions;
using BE.Emulator.Framework;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.ProfileSettings
{
/// <summary>
/// View wrapper for the profile settings modal.
/// </summary>
internal sealed class ProfileSettingsModalView : BaseView<ProfileSettingsModalViewModel>
{
    /// <summary>
    /// The USS root class applied to the modal.
    /// </summary>
    public const string RootUssClassName = "profile-settings-modal-root";

    /// <summary>
    /// The USS panel class applied to the settings surface.
    /// </summary>
    public const string PanelUssClassName = "profile-settings-modal";

    /// <summary>
    /// The element name used for the backdrop dismiss target.
    /// </summary>
    public const string BackdropName = "dismiss-profile-settings";

    /// <summary>
    /// The element name used for the close button.
    /// </summary>
    public const string CloseButtonName = "close-profile-settings";

    /// <summary>
    /// The element name used for the profile avatar.
    /// </summary>
    public const string AvatarName = "profile-settings-avatar";

    /// <summary>
    /// The element name used for the profile display name label.
    /// </summary>
    public const string DisplayNameLabelName = "profile-settings-display-name";

    /// <summary>
    /// The element name used for the edit-profile button.
    /// </summary>
    public const string EditProfileButtonName = "edit-profile";

    /// <summary>
    /// The element name used for the set-pin button.
    /// </summary>
    public const string SetPinButtonName = "set-pin-button";

    /// <summary>
    /// The element name used for the delete-profile button.
    /// </summary>
    public const string DeleteProfileButtonName = "delete-profile-button";

    /// <summary>
    /// The modifier applied when the modal is visible.
    /// </summary>
    public const string OpenModifierName = "--open";

    /// <summary>
    /// The modifier applied when a local toggle is on.
    /// </summary>
    public const string ToggleOnModifierName = "--on";

    private const string AdminToggleName = "admin-toggle";
    private const string AdminToggleTextName = "admin-toggle-text";
    private const string PlayTimeLimitsToggleName = "play-time-limits-toggle";
    private const string PlayTimeLimitsToggleTextName = "play-time-limits-toggle-text";
    private const string PlayTimeWindowsToggleName = "play-time-windows-toggle";
    private const string PlayTimeWindowsToggleTextName = "play-time-windows-toggle-text";
    private const string AllowPurchasesToggleName = "allow-purchases-toggle";
    private const string AllowPurchasesToggleTextName = "allow-purchases-toggle-text";

    /// <inheritdoc />
    protected override string StyleClassName => RootUssClassName;

    private readonly List<Action> _unbindToggleCallbacks = new();
    private VisualElement _root;
    private VisualElement _panel;
    private VisualElement _avatar;
    private Label _displayNameLabel;
    private Button _editProfileButton;
    private Button _setPinButton;
    private Button _deleteProfileButton;
    private VisualElement _adminToggle;
    private Label _adminToggleText;
    private VisualElement _playTimeLimitsToggle;
    private Label _playTimeLimitsToggleText;
    private VisualElement _playTimeWindowsToggle;
    private Label _playTimeWindowsToggleText;
    private VisualElement _allowPurchasesToggle;
    private Label _allowPurchasesToggleText;
    private EventCallback<PointerDownEvent> _stopPropagationCallback;
    private ProfileSettingsModalState _state = new();
    private bool _isAdminEnabled;
    private bool _isPlayTimeLimitsEnabled;
    private bool _isPlayTimeWindowsEnabled;
    private bool _isAllowPurchasesEnabled;

    /// <summary>
    /// Creates the profile settings modal view.
    /// </summary>
    public ProfileSettingsModalView([NotNull] ILoggerFactory loggerFactory, [NotNull] ProfileSettingsModalViewModel viewModel)
        : base(loggerFactory, viewModel.SourceTemplate, viewModel)
    {
    }

    /// <summary>
    /// Applies the supplied selected-profile state to the view.
    /// </summary>
    public void SetProfileState(ProfileSettingsModalState state)
    {
        _state = state ?? new ProfileSettingsModalState();
        ApplyProfileState();
    }

    /// <inheritdoc />
    protected override void Bind(VisualElement host, VisualElement root)
    {
        _root = root;
        _panel = root.Q(className: PanelUssClassName);
        _avatar = root.Q(AvatarName);
        _displayNameLabel = root.Q<Label>(DisplayNameLabelName);
        _editProfileButton = root.Q<Button>(EditProfileButtonName);
        _setPinButton = root.Q<Button>(SetPinButtonName);
        _deleteProfileButton = root.Q<Button>(DeleteProfileButtonName);
        _adminToggle = root.Q(AdminToggleName);
        _adminToggleText = root.Q<Label>(AdminToggleTextName);
        _playTimeLimitsToggle = root.Q(PlayTimeLimitsToggleName);
        _playTimeLimitsToggleText = root.Q<Label>(PlayTimeLimitsToggleTextName);
        _playTimeWindowsToggle = root.Q(PlayTimeWindowsToggleName);
        _playTimeWindowsToggleText = root.Q<Label>(PlayTimeWindowsToggleTextName);
        _allowPurchasesToggle = root.Q(AllowPurchasesToggleName);
        _allowPurchasesToggleText = root.Q<Label>(AllowPurchasesToggleTextName);

        VisualElement backdrop = root.Q(BackdropName);
        VisualElement closeButton = root.Q(CloseButtonName, EmulatorView.ButtonUssClassName);

        _stopPropagationCallback = evt => evt.StopPropagation();
        _root.RegisterCallback(_stopPropagationCallback);

        RegisterViewAction<PointerDownEvent>(backdrop, new CloseProfileSettingsViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<PointerDownEvent>(closeButton, new CloseProfileSettingsViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(closeButton, new CloseProfileSettingsViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction(_editProfileButton, () => new OpenEditProfileViewAction
        {
            Mode = EditProfileMode.Edit,
            SelectedProfileId = _state.SelectedProfileId
        });
        RegisterViewAction(_setPinButton, () => new OpenSetPinViewAction
        {
            SelectedProfileId = _state.SelectedProfileId
        });
        RegisterViewAction(_deleteProfileButton, () => new OpenDeleteProfileViewAction
        {
            SelectedProfileId = _state.SelectedProfileId
        });

        RegisterLocalToggle(_adminToggle, () => _isAdminEnabled = !_isAdminEnabled);
        RegisterLocalToggle(_playTimeLimitsToggle, () => _isPlayTimeLimitsEnabled = !_isPlayTimeLimitsEnabled);
        RegisterLocalToggle(_playTimeWindowsToggle, () => _isPlayTimeWindowsEnabled = !_isPlayTimeWindowsEnabled);
        RegisterLocalToggle(_allowPurchasesToggle, () => _isAllowPurchasesEnabled = !_isAllowPurchasesEnabled);

        ResetToggleState();
        ApplyProfileState();
    }

    /// <inheritdoc />
    protected override void Unbind(VisualElement host, VisualElement root)
    {
        foreach(Action unbind in _unbindToggleCallbacks)
        {
            unbind.Invoke();
        }

        if(_root != null && _stopPropagationCallback != null)
        {
            _root.UnregisterCallback(_stopPropagationCallback);
        }

        _unbindToggleCallbacks.Clear();
        _root = null;
        _panel = null;
        _avatar = null;
        _displayNameLabel = null;
        _editProfileButton = null;
        _setPinButton = null;
        _deleteProfileButton = null;
        _adminToggle = null;
        _adminToggleText = null;
        _playTimeLimitsToggle = null;
        _playTimeLimitsToggleText = null;
        _playTimeWindowsToggle = null;
        _playTimeWindowsToggleText = null;
        _allowPurchasesToggle = null;
        _allowPurchasesToggleText = null;
        _stopPropagationCallback = null;
    }

    /// <inheritdoc />
    protected override void ShowView()
    {
        ResetToggleState();
        ApplyProfileState();
        _root.AddToClassList(OpenModifierName);
    }

    /// <inheritdoc />
    protected override void HideView()
    {
        _root.RemoveFromClassList(OpenModifierName);
    }

    private void RegisterLocalToggle(VisualElement toggle, Action toggleAction)
    {
        if(toggle == null || toggleAction == null)
        {
            return;
        }

        EventCallback<ClickEvent> callback = evt =>
        {
            evt.StopPropagation();
            toggleAction.Invoke();
            RefreshToggleVisuals();
        };

        toggle.RegisterCallback(callback, TrickleDown.TrickleDown);
        _unbindToggleCallbacks.Add(() => toggle.UnregisterCallback(callback, TrickleDown.TrickleDown));
    }

    private void ApplyProfileState()
    {
        if(_displayNameLabel != null)
        {
            _displayNameLabel.text = _state.DisplayName ?? string.Empty;
        }

        if(_avatar != null)
        {
            _avatar.style.backgroundImage = _state.AvatarImage;
            _avatar.style.backgroundColor = _state.AvatarBackgroundColor;
        }
    }

    private void ResetToggleState()
    {
        _isAdminEnabled = false;
        _isPlayTimeLimitsEnabled = false;
        _isPlayTimeWindowsEnabled = false;
        _isAllowPurchasesEnabled = false;
        RefreshToggleVisuals();
    }

    private void RefreshToggleVisuals()
    {
        RefreshToggle(_adminToggle, _adminToggleText, _isAdminEnabled);
        RefreshToggle(_playTimeLimitsToggle, _playTimeLimitsToggleText, _isPlayTimeLimitsEnabled);
        RefreshToggle(_playTimeWindowsToggle, _playTimeWindowsToggleText, _isPlayTimeWindowsEnabled);
        RefreshToggle(_allowPurchasesToggle, _allowPurchasesToggleText, _isAllowPurchasesEnabled);
    }

    private static void RefreshToggle(VisualElement toggle, Label toggleText, bool isOn)
    {
        toggle?.EnableInClassList(ToggleOnModifierName, isOn);
        if(toggleText != null)
        {
            toggleText.text = isOn ? "On" : "Off";
        }
    }
}
}
