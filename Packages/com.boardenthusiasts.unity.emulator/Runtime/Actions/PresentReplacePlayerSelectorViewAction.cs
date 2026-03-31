using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the replace-player selector modal be shown for a specific session player.
/// </summary>
internal sealed class PresentReplacePlayerSelectorViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the target session identifier being replaced or removed.
    /// </summary>
    public int TargetSessionId { get; set; }
}
}
