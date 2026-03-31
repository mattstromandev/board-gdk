using BE.Emulator.Data;
using BE.Emulator.Framework;

namespace BE.Emulator.Modals.DeleteProfile
{
/// <summary>
/// Backing model for the delete-profile modal.
/// </summary>
internal sealed class DeleteProfileModalModel : IModel
{
    /// <summary>
    /// Creates a view-state snapshot for the provided profile.
    /// </summary>
    public DeleteProfileModalState CreateState(EmulatorProfileData profile)
    {
        string displayName = profile?.DisplayName ?? string.Empty;
        return new DeleteProfileModalState
        {
            SelectedProfileId = profile?.PlayerId ?? string.Empty,
            DisplayName = displayName,
            TitleText = $"Delete \"{displayName}\" profile?",
            BodyText = $"All of {displayName}'s game history will be deleted.\nThis cannot be undone."
        };
    }
}

/// <summary>
/// View-state snapshot for the delete-profile modal.
/// </summary>
internal sealed class DeleteProfileModalState
{
    /// <summary>
    /// The selected profile identifier.
    /// </summary>
    public string SelectedProfileId { get; set; }

    /// <summary>
    /// The selected profile display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The fully formatted modal title.
    /// </summary>
    public string TitleText { get; set; }

    /// <summary>
    /// The fully formatted modal body copy.
    /// </summary>
    public string BodyText { get; set; }
}
}
