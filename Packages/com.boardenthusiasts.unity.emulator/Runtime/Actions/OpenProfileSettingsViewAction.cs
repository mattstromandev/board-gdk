using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the selected profile's settings modal be shown.
/// </summary>
internal sealed class OpenProfileSettingsViewAction : IViewAction
{
    /// <summary>
    /// The identifier of the profile whose settings should be shown.
    /// </summary>
    public string SelectedProfileId { get; set; }
}
}
