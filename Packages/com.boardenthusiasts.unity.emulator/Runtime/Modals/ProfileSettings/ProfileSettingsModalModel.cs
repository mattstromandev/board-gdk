using BE.Emulator.Data;
using BE.Emulator.Framework;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.ProfileSettings
{
/// <summary>
/// Backing model for the profile settings modal.
/// </summary>
internal sealed class ProfileSettingsModalModel : IModel
{
    /// <summary>
    /// Creates a view-state snapshot for the provided profile.
    /// </summary>
    public ProfileSettingsModalState CreateState(EmulatorProfileData profile)
    {
        return new ProfileSettingsModalState
        {
            SelectedProfileId = profile?.PlayerId ?? string.Empty,
            DisplayName = profile?.DisplayName ?? string.Empty,
            AvatarImage = new StyleBackground(profile?.Avatar),
            AvatarBackgroundColor = new StyleColor(profile?.AvatarBackgroundColor ?? default)
        };
    }
}

/// <summary>
/// View-state snapshot for the profile settings modal.
/// </summary>
internal sealed class ProfileSettingsModalState
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
    /// The selected profile avatar image.
    /// </summary>
    public StyleBackground AvatarImage { get; set; }

    /// <summary>
    /// The selected profile avatar background color.
    /// </summary>
    public StyleColor AvatarBackgroundColor { get; set; }
}
}
