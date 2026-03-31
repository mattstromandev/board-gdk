using System;
using System.Collections.Generic;

using BE.Emulator.Framework;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.ManageProfiles
{
/// <summary>
/// View model for the manage profiles modal.
/// </summary>
[Serializable]
internal sealed class ManageProfilesModalViewModel : IViewModel
{
    private readonly List<ManageProfilesProfileItemViewModel> _profileItems = new();

    /// <summary>
    /// The template to clone when constructing the manage profiles modal view tree.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }

    /// <summary>
    /// The template to clone when constructing each manage profiles list item.
    /// </summary>
    public VisualTreeAsset ProfileItemTemplate { get; set; }

    /// <summary>
    /// The profile items that should be rendered by the manage profiles modal.
    /// </summary>
    public IReadOnlyList<ManageProfilesProfileItemViewModel> ProfileItems => _profileItems;

    /// <summary>
    /// Replace the currently rendered profile items.
    /// </summary>
    public void SetProfileItems(IEnumerable<ManageProfilesProfileItemViewModel> profileItems)
    {
        _profileItems.Clear();

        if(profileItems == null)
        {
            return;
        }

        _profileItems.AddRange(profileItems);
    }
}
}
