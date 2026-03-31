using System;
using System.Collections.Generic;

using BE.Emulator.Actions;
using BE.Emulator.Data;
using BE.Emulator.Framework;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

namespace BE.Emulator
{
/// <summary>
/// View that renders the runtime Emulator shell and provides display host elements.
/// </summary>
internal sealed class EmulatorView : BaseView<EmulatorViewModel>
{
    /// <summary>
    /// The USS root class applied to the shell.
    /// </summary>
    public const string UssClassName = "board-os";
    /// <summary>
    /// The USS class applied to shell buttons.
    /// </summary>
    public const string ButtonUssClassName = UssClassName + "__button";
    /// <summary>
    /// The USS class applied to the screen host frame.
    /// </summary>
    public const string ScreenFrameUssClassName = UssClassName + "__screen-frame";
    /// <summary>
    /// The USS class applied to the modal content host frame.
    /// </summary>
    public const string ModalContentFrameUssClassName = UssClassName + "__modal-content-frame";
    /// <summary>
    /// The modifier used when safe-space visualization should be shown.
    /// </summary>
    public const string ShowSafeSpaceModifierName = "--show-safe-space";
    /// <summary>
    /// The modifier used when the profile switcher affordance should be enabled.
    /// </summary>
    public const string ProfileSwitchEnabledModifierName = "--profile-switch-enabled";
    /// <summary>
    /// The modifier used when settings access should be enabled.
    /// </summary>
    public const string SettingsEnabledModifierName = "--settings-enabled";
    /// <summary>
    /// The modifier used when any modal is visible.
    /// </summary>
    public const string ModalOpenModifierName = "--modal-open";
    /// <summary>
    /// The modifier used when any screen is visible.
    /// </summary>
    public const string ScreenOpenModifierName = "--screen-open";
    /// <summary>
    /// The prefix used for dedicated display host element names.
    /// </summary>
    public const string DisplayHostNamePrefix = "host--";
    /// <summary>
    /// The shell element name for the profile switcher button.
    /// </summary>
    public const string SwitchProfileButtonName = "switch-profile";
    /// <summary>
    /// The shell element name for the settings button.
    /// </summary>
    public const string OpenSettingsButtonName = "open-settings";
    /// <summary>
    /// The shell element name for the screens host container.
    /// </summary>
    public const string ScreensHostName = "screens";
    /// <summary>
    /// The shell element name for the modal overlay host container.
    /// </summary>
    public const string ModalOverlayHostName = "modals";
    /// <summary>
    /// The shell element name for the modal content host container.
    /// </summary>
    public const string ModalHostName = "modals-content";

    /// <inheritdoc />
    protected override string StyleClassName => UssClassName;

    private readonly IRahmenLogger _logger;
    private VisualElement _root;
    private VisualElement _switchProfileButton;
    private Button _openSettingsButton;
    private VisualElement _screensHost;
    private VisualElement _modalHost;

    public EmulatorView([NotNull] ILoggerFactory loggerFactory, [NotNull] EmulatorViewModel viewModel)
        : base(loggerFactory, viewModel.SourceTemplate, viewModel)
    {
        _logger = loggerFactory?.Get<LogChannels.BoardEmulation>(this) ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// The host element for Board OS screens.
    /// </summary>
    public VisualElement ScreensHost => _screensHost ?? throw new InvalidOperationException("Screens host is not available until the shell is attached.");
    /// <summary>
    /// The host element for Board OS modals.
    /// </summary>
    public VisualElement ModalHost => _modalHost ?? throw new InvalidOperationException("Modal host is not available until the shell is attached.");

    /// <summary>
    /// Updates the shell profile summary from the active Board profile.
    /// </summary>
    /// <param name="activeProfile">The active Board profile to reflect in the shell header.</param>
    public void SetActiveProfile(EmulatorProfileData activeProfile)
    {
        ViewModel.Username = activeProfile?.DisplayName ?? string.Empty;
        ViewModel.AvatarImage = new StyleBackground(activeProfile?.Avatar);
        ViewModel.AvatarBackgroundColor = new StyleColor(activeProfile?.AvatarBackgroundColor ?? default);
    }

    /// <summary>
    /// Shows or hides the profile switcher affordance in the shell.
    /// </summary>
    /// <param name="isVisible"><see langword="true"/> to show the affordance; otherwise, <see langword="false"/>.</param>
    public void SetProfileSwitcherVisible(bool isVisible)
    {
        if(_root == null)
        {
            return;
        }

        _root.EnableInClassList(ProfileSwitchEnabledModifierName, isVisible);
    }

    /// <summary>
    /// Shows or hides the safe-space visualization overlay.
    /// </summary>
    /// <param name="isVisible"><see langword="true"/> to show the overlay; otherwise, <see langword="false"/>.</param>
    public void SetSafeSpaceVisible(bool isVisible)
    {
        if(_root == null)
        {
            return;
        }

        _root.EnableInClassList(ShowSafeSpaceModifierName, isVisible);
    }

    /// <summary>
    /// Refreshes shell-level display state modifiers based on the currently registered displays.
    /// </summary>
    /// <param name="displays">The displays whose visibility state should be reflected into the shell.</param>
    public void RefreshDisplayState([NotNull] IEnumerable<IEmulatorDisplay> displays)
    {
        if(displays == null)
        {
            throw new ArgumentNullException(nameof(displays));
        }

        if(_root == null)
        {
            return;
        }

        bool anyModalVisible = false;
        bool anyScreenVisible = false;

        foreach(IEmulatorDisplay display in displays)
        {
            _root.RemoveFromClassList(GetScopedModifierName(ModalOpenModifierName, display.Name));
            _root.RemoveFromClassList(GetScopedModifierName(ScreenOpenModifierName, display.Name));

            if(display.IsVisible == false)
            {
                continue;
            }

            if(display is Modals.IEmulatorModal)
            {
                anyModalVisible = true;
                _root.AddToClassList(GetScopedModifierName(ModalOpenModifierName, display.Name));
            }
            else if(display is Screens.IEmulatorScreen)
            {
                anyScreenVisible = true;
                _root.AddToClassList(GetScopedModifierName(ScreenOpenModifierName, display.Name));
            }
        }

        _root.EnableInClassList(ModalOpenModifierName, anyModalVisible);
        _root.EnableInClassList(ScreenOpenModifierName, anyScreenVisible);
    }

    /// <summary>
    /// Attempts to resolve a dedicated host element for the specified display.
    /// </summary>
    /// <param name="displayName">The logical display name.</param>
    /// <param name="host">When this method returns, contains the dedicated host element if one exists.</param>
    /// <returns><see langword="true"/> when a dedicated host element was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetDedicatedDisplayHost([NotNull] string displayName, out VisualElement host)
    {
        if(displayName == null)
        {
            throw new ArgumentNullException(nameof(displayName));
        }

        host = _root?.Q(GetDisplayHostName(displayName));
        return host != null;
    }

    /// <summary>
    /// Gets the shell element name used for a dedicated display host.
    /// </summary>
    /// <param name="displayName">The logical display name.</param>
    /// <returns>The dedicated host element name for <paramref name="displayName"/>.</returns>
    public static string GetDisplayHostName([NotNull] string displayName)
    {
        if(displayName == null)
        {
            throw new ArgumentNullException(nameof(displayName));
        }

        return DisplayHostNamePrefix + displayName;
    }

    /// <inheritdoc />
    protected override void Bind(VisualElement host, VisualElement root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
        _root.dataSource = ViewModel;
        _root.EnableInClassList(SettingsEnabledModifierName, true);

        _switchProfileButton = root.Q(SwitchProfileButtonName, ButtonUssClassName);
        _openSettingsButton = root.Q<Button>(OpenSettingsButtonName, ButtonUssClassName);
        _screensHost = root.Q(ScreensHostName, ScreenFrameUssClassName);
        _modalHost = root.Q(ModalHostName, ModalContentFrameUssClassName);

        if(_screensHost == null || _modalHost == null)
        {
            _logger.Error()?.Log("Shell view could not resolve required display host elements.");
        }

        RegisterViewAction<ClickEvent>(_switchProfileButton, new OpenProfileSwitcherViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction(_openSettingsButton, new OpenQuickSettingsViewAction());
    }

    /// <inheritdoc />
    protected override void Unbind(VisualElement host, VisualElement root)
    {
        _switchProfileButton = null;
        _openSettingsButton = null;
        _screensHost = null;
        _modalHost = null;
        _root = null;
    }

    /// <inheritdoc />
    protected override void ShowView()
    {
    }

    /// <inheritdoc />
    protected override void HideView()
    {
    }

    private static string GetScopedModifierName([NotNull] string modifierName, [NotNull] string displayName)
    {
        if(modifierName == null)
        {
            throw new ArgumentNullException(nameof(modifierName));
        }

        if(displayName == null)
        {
            throw new ArgumentNullException(nameof(displayName));
        }

        return modifierName + "__" + displayName;
    }
}
}
