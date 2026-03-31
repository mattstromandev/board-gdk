using BE.Emulator.Framework;

using JetBrains.Annotations;

using Rahmen.Logging;

using Zenject;

namespace BE.Emulator.Modals.QuickSettings
{
/// <summary>
/// Controller for the quick settings modal.
/// </summary>
internal sealed class QuickSettingsModal
    : DisplayableController<QuickSettingsModalModel, QuickSettingsModalView, QuickSettingsModalViewModel>, IEmulatorModal
{
    /// <summary>
    /// The logical display name for the quick settings modal.
    /// </summary>
    public const string DisplayName = "quick-settings";

    /// <inheritdoc />
    public string Name => DisplayName;

    /// <summary>
    /// Creates the quick settings modal controller.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="model">The backing model used by the controller.</param>
    /// <param name="view">The view controlled by this controller.</param>
    /// <param name="displayActionRouter">The router that should handle published view actions.</param>
    public QuickSettingsModal(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] QuickSettingsModalModel model,
        [NotNull] QuickSettingsModalView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter)
        : base(loggerFactory, model, view, displayActionRouter)
    {
    }
}
}
