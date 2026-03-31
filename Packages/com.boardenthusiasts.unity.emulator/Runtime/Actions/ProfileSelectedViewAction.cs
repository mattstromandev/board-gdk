using BE.Emulator.Framework;
using BE.Emulator.Screens.ProfileSwitcher;

namespace BE.Emulator.Actions
{
/// <summary>
/// Informs that a new profile has been selected.
/// </summary>
internal sealed class ProfileSelectedViewAction : IViewAction
{
    /// <summary>
    /// The identifier of the profile that was selected.
    /// </summary>
    public string SelectedProfileId { get; set; }
}
}
