using BE.Emulator.Actions;
using BE.Emulator.Data;
using BE.Emulator.Framework;
using BE.Emulator.Utility;

using UnityEngine;

namespace BE.Emulator.Modals.EditProfile
{
/// <summary>
/// Backing model for the shared create and edit profile modal.
/// </summary>
internal sealed class EditProfileModalModel : IModel
{
    /// <summary>
    /// The minimum supported nickname length.
    /// </summary>
    public const int MinNicknameLength = 2;

    /// <summary>
    /// The maximum supported nickname length.
    /// </summary>
    public const int MaxNicknameLength = 14;

    /// <summary>
    /// The validation guidance shown below the nickname field.
    /// </summary>
    public const string ValidationMessage = "Profile name must be 2-14 characters, only letters and numbers.";

    /// <summary>
    /// The duplicate-name guidance shown after a failed save attempt.
    /// </summary>
    public const string DuplicateValidationMessage = "You've already used this nickname for another profile. Try a different one.";

    /// <summary>
    /// Creates the default create-profile state.
    /// </summary>
    public EditProfileModalState CreateCreateState()
    {
        return new EditProfileModalState
        {
            Mode = EditProfileMode.Create,
            Title = "Add profile",
            SelectedProfileId = string.Empty,
            DisplayName = string.Empty,
            AvatarBackgroundColor = GetDefaultAvatarBackgroundColor()
        };
    }

    /// <summary>
    /// Creates the edit-profile state for an existing profile.
    /// </summary>
    public EditProfileModalState CreateEditState(EmulatorProfileData profile)
    {
        Color avatarBackgroundColor = EmulatorAvatarUtility.HasSerializedColor(profile?.AvatarBackgroundColor ?? default)
            ? profile.AvatarBackgroundColor
            : GetDefaultAvatarBackgroundColor();

        return new EditProfileModalState
        {
            Mode = EditProfileMode.Edit,
            Title = "Edit Profile",
            SelectedProfileId = profile?.PlayerId ?? string.Empty,
            DisplayName = profile?.DisplayName ?? string.Empty,
            AvatarBackgroundColor = avatarBackgroundColor
        };
    }

    /// <summary>
    /// Gets the default avatar background color shown when the modal opens.
    /// </summary>
    public Color GetDefaultAvatarBackgroundColor()
    {
        return EmulatorAvatarUtility.GetPaletteColor(0);
    }

    /// <summary>
    /// Gets a random avatar background color that differs from the provided current color when possible.
    /// </summary>
    public Color GetRandomAvatarBackgroundColor(Color currentColor)
    {
        return EmulatorAvatarUtility.GetRandomPaletteColor(currentColor);
    }

    /// <summary>
    /// Gets whether the supplied nickname satisfies the Board OS profile naming rules.
    /// </summary>
    public bool IsValidNickname(string nickname)
    {
        if(string.IsNullOrEmpty(nickname))
        {
            return false;
        }

        if(nickname.Length < MinNicknameLength || nickname.Length > MaxNicknameLength)
        {
            return false;
        }

        foreach(char character in nickname)
        {
            if(char.IsLetterOrDigit(character) == false)
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Open-state snapshot for the shared edit-profile modal.
/// </summary>
internal sealed class EditProfileModalState
{
    /// <summary>
    /// The mode that the modal should operate in.
    /// </summary>
    public EditProfileMode Mode { get; set; }

    /// <summary>
    /// The selected profile identifier when editing an existing profile.
    /// </summary>
    public string SelectedProfileId { get; set; }

    /// <summary>
    /// The title shown at the top of the modal.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The authored display name shown in the text field when the modal opens.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The authored avatar background color shown when the modal opens.
    /// </summary>
    public Color AvatarBackgroundColor { get; set; }
}
}
