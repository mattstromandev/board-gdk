using UnityEngine;

using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the shared edit-profile modal persist its current draft.
/// </summary>
internal sealed class EditProfileSavedViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the modal mode that produced the save request.
    /// </summary>
    public EditProfileMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the selected profile identifier when saving an existing profile.
    /// </summary>
    public string SelectedProfileId { get; set; }

    /// <summary>
    /// Gets or sets the display name that should be persisted.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the internally persisted avatar background color.
    /// </summary>
    public Color AvatarBackgroundColor { get; set; }
}
}
