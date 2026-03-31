using System;

using UnityEngine;
using UnityEngine.UIElements;

namespace BE.Emulator
{
/// <summary>
/// Serializable settings asset that configures the runtime BE Emulator for Board shell.
/// </summary>
[CreateAssetMenu(menuName = "Board Enthusiasts/Emulator/Emulator Settings")]
[Serializable]
public class EmulatorSettings : ScriptableObject, IEmulatorSettings
{
    [field: SerializeField]
    [field: Tooltip("The " + nameof(PanelSettings) + " to be used for the host " + nameof(UIDocument) + ".")]
    /// <inheritdoc />
    public PanelSettings PanelSettings { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The sort order to be used for the host " + nameof(UIDocument) + ".")]
    /// <inheritdoc />
    public float SortOrder { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " to be used as the root for the host " + nameof(UIDocument) + ".")]
    /// <inheritdoc />
    public VisualTreeAsset SourceTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create the profile switcher.")]
    /// <inheritdoc />
    public VisualTreeAsset ProfileSwitcherTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create each profile switcher card.")]
    /// <inheritdoc />
    public VisualTreeAsset ProfileSwitcherProfileCardTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create the quick settings modal.")]
    /// <inheritdoc />
    public VisualTreeAsset QuickSettingsModalTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create the manage profiles modal.")]
    /// <inheritdoc />
    public VisualTreeAsset ManageProfilesModalTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create the profile settings modal.")]
    /// <inheritdoc />
    public VisualTreeAsset ProfileSettingsModalTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create the shared edit-profile modal.")]
    /// <inheritdoc />
    public VisualTreeAsset EditProfileModalTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create the delete-profile modal.")]
    /// <inheritdoc />
    public VisualTreeAsset DeleteProfileModalTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create the add-player modal.")]
    /// <inheritdoc />
    public VisualTreeAsset AddPlayerModalTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create the replace-player modal.")]
    /// <inheritdoc />
    public VisualTreeAsset ReplacePlayerModalTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create each add-player card.")]
    /// <inheritdoc />
    public VisualTreeAsset AddPlayerCardTemplate { get; private set; }

    [field: SerializeField]
    [field: Tooltip("The " + nameof(VisualTreeAsset) + " template which will be used to create each manage profiles list item.")]
    /// <inheritdoc />
    public VisualTreeAsset ManageProfilesProfileItemTemplate { get; private set; }
}
}
