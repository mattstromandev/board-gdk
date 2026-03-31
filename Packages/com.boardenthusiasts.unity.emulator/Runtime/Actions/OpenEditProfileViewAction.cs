using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// The supported modes for the shared edit-profile modal.
/// </summary>
internal enum EditProfileMode
{
    /// <summary>
    /// Author a new profile.
    /// </summary>
    Create,

    /// <summary>
    /// Edit an existing profile.
    /// </summary>
    Edit
}

/// <summary>
/// Requests that the shared edit-profile modal be shown.
/// </summary>
internal sealed class OpenEditProfileViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the mode that the modal should open in.
    /// </summary>
    public EditProfileMode Mode { get; set; } = EditProfileMode.Create;

    /// <summary>
    /// Gets or sets the selected profile identifier when opening in edit mode.
    /// </summary>
    public string SelectedProfileId { get; set; }
}
}
