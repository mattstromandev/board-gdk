using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the safe-space overlay visibility be updated.
/// </summary>
public sealed class SetSafeSpaceVisibilityViewAction : IViewAction
{
    /// <summary>
    /// Whether the safe-space overlay should be visible.
    /// </summary>
    public bool IsVisible { get; set; }
}
}
