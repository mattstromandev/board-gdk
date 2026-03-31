using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the selected session player be removed from the current session.
/// </summary>
internal sealed class PlayerRemovedViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the target session identifier to remove.
    /// </summary>
    public int TargetSessionId { get; set; }
}
}
