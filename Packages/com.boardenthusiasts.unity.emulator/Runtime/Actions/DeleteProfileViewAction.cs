using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the selected profile be deleted.
/// </summary>
internal sealed class DeleteProfileViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the selected profile identifier.
    /// </summary>
    public string SelectedProfileId { get; set; }
}
}
