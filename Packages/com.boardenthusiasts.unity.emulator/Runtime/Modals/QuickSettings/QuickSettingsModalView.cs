using BE.Emulator.Actions;
using BE.Emulator.Framework;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.QuickSettings
{
/// <summary>
/// View wrapper for the quick settings modal.
/// </summary>
internal sealed class QuickSettingsModalView : BaseView<QuickSettingsModalViewModel>
{
    /// <summary>
    /// The USS root class applied to the modal.
    /// </summary>
    public const string UssClassName = "quick-settings-modal";
    /// <summary>
    /// The element name used for the power button.
    /// </summary>
    public const string PowerButtonName = "power";
    /// <summary>
    /// The element name used for the sleep button.
    /// </summary>
    public const string SleepButtonName = "sleep";
    /// <summary>
    /// The element name used for the wifi button.
    /// </summary>
    public const string WifiButtonName = "wifi";
    /// <summary>
    /// The element name used for the display button.
    /// </summary>
    public const string DisplayButtonName = "display";
    /// <summary>
    /// The element name used for the audio button.
    /// </summary>
    public const string AudioButtonName = "audio";
    /// <summary>
    /// The element name used for the settings button.
    /// </summary>
    public const string SettingsButtonName = "settings";
    /// <summary>
    /// The modifier applied when the modal is visible.
    /// </summary>
    public const string OpenModifierName = "--open";

    /// <inheritdoc />
    protected override string StyleClassName => UssClassName;

    private VisualElement _root;
    private VisualElement _powerButton;
    private VisualElement _sleepButton;
    private VisualElement _wifiButton;
    private VisualElement _displayButton;
    private VisualElement _audioButton;
    private VisualElement _settingsButton;
    private EventCallback<PointerDownEvent> _stopPropagationCallback;

    /// <summary>
    /// Creates the quick settings modal view.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="viewModel">The view model bound to this view.</param>
    public QuickSettingsModalView([NotNull] ILoggerFactory loggerFactory, [NotNull] QuickSettingsModalViewModel viewModel)
        : base(loggerFactory, viewModel.SourceTemplate, viewModel)
    {
    }

    /// <inheritdoc />
    protected override void Bind(VisualElement host, VisualElement root)
    {
        _root = root;
        VisualElement dismissTarget = host?.parent ?? host;
        _powerButton = root.Q(PowerButtonName, EmulatorView.ButtonUssClassName);
        _sleepButton = root.Q(SleepButtonName, EmulatorView.ButtonUssClassName);
        _wifiButton = root.Q(WifiButtonName, EmulatorView.ButtonUssClassName);
        _displayButton = root.Q(DisplayButtonName, EmulatorView.ButtonUssClassName);
        _audioButton = root.Q(AudioButtonName, EmulatorView.ButtonUssClassName);
        _settingsButton = root.Q(SettingsButtonName, EmulatorView.ButtonUssClassName);

        _stopPropagationCallback = evt => evt.StopPropagation();
        _root.RegisterCallback(_stopPropagationCallback);
        RegisterViewAction<PointerDownEvent>(dismissTarget, new CloseQuickSettingsViewAction());
        RegisterViewAction<ClickEvent>(_powerButton, new QuickSettingsPowerViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(_sleepButton, new QuickSettingsSleepViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(_wifiButton, new QuickSettingsWifiViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(_displayButton, new QuickSettingsDisplayViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(_audioButton, new QuickSettingsAudioViewAction(), TrickleDown.TrickleDown);
        RegisterViewAction<ClickEvent>(_settingsButton, new QuickSettingsSettingsViewAction(), TrickleDown.TrickleDown);
    }

    /// <inheritdoc />
    protected override void Unbind(VisualElement host, VisualElement root)
    {
        if(_root != null && _stopPropagationCallback != null)
        {
            _root.UnregisterCallback(_stopPropagationCallback);
        }

        _powerButton = null;
        _sleepButton = null;
        _wifiButton = null;
        _displayButton = null;
        _audioButton = null;
        _settingsButton = null;
        _stopPropagationCallback = null;
    }

    /// <inheritdoc />
    protected override void ShowView()
    {
        _root.AddToClassList(OpenModifierName);
    }

    /// <inheritdoc />
    protected override void HideView()
    {
        _root.RemoveFromClassList(OpenModifierName);
    }
}
}
