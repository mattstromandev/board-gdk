using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the selected session player be replaced with a persistent profile.
/// </summary>
internal sealed class PlayerReplacedViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the target session identifier being replaced.
    /// </summary>
    public int TargetSessionId { get; set; }

    /// <summary>
    /// Gets or sets the persistent profile identifier that should replace the target session player.
    /// </summary>
    public string SelectedProfileId { get; set; }
}
}
