using UnityEngine.UIElements;

namespace BE.Emulator
{
/// <summary>
/// Editor configuration contract for the Board OS emulator shell and child screens.
/// </summary>
public interface IEmulatorSettings
{
    /// <summary>
    /// The <see cref="PanelSettings"/> to be used for the host <see cref="UIDocument"/>.
    /// </summary>
    public PanelSettings PanelSettings { get; }

    /// <summary>
    /// The sort order to be used for the host <see cref="UIDocument"/>.
    /// </summary>
    public float SortOrder { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> to be used as the root for the host <see cref="UIDocument"/>.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create the profile switcher.
    /// </summary>
    public VisualTreeAsset ProfileSwitcherTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create each profile switcher card.
    /// </summary>
    public VisualTreeAsset ProfileSwitcherProfileCardTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create the quick settings modal.
    /// </summary>
    public VisualTreeAsset QuickSettingsModalTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create the manage profiles modal.
    /// </summary>
    public VisualTreeAsset ManageProfilesModalTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create the profile settings modal.
    /// </summary>
    public VisualTreeAsset ProfileSettingsModalTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create the shared edit-profile modal.
    /// </summary>
    public VisualTreeAsset EditProfileModalTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create the delete-profile modal.
    /// </summary>
    public VisualTreeAsset DeleteProfileModalTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create the add-player modal.
    /// </summary>
    public VisualTreeAsset AddPlayerModalTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create the replace-player modal.
    /// </summary>
    public VisualTreeAsset ReplacePlayerModalTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create each add-player card.
    /// </summary>
    public VisualTreeAsset AddPlayerCardTemplate { get; }

    /// <summary>
    /// The <see cref="VisualTreeAsset"/> template which will be used to create each manage profiles list item.
    /// </summary>
    public VisualTreeAsset ManageProfilesProfileItemTemplate { get; }
}
}
