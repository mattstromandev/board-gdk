using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the delete-profile flow be opened for the selected profile.
/// </summary>
internal sealed class OpenDeleteProfileViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the selected profile identifier.
    /// </summary>
    public string SelectedProfileId { get; set; }
}
}
