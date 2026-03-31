using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the set-pin flow be opened for the selected profile.
/// </summary>
internal sealed class OpenSetPinViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the selected profile identifier.
    /// </summary>
    public string SelectedProfileId { get; set; }
}
}
