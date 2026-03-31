using System;
using System.Collections.Generic;
using System.Linq;

using BE.Emulator.Framework;

namespace BE.Emulator.Modals.ManageProfiles
{
/// <summary>
/// Backing model for the manage profiles modal.
/// </summary>
internal sealed class ManageProfilesModalModel : IModel
{
    private IReadOnlyList<ManageProfilesProfileItemViewModel> _profileItems = Array.Empty<ManageProfilesProfileItemViewModel>();

    /// <summary>
    /// The profile items currently represented by the modal.
    /// </summary>
    public IReadOnlyList<ManageProfilesProfileItemViewModel> ProfileItems => _profileItems;

    /// <summary>
    /// Replace the current profile items with the provided set.
    /// </summary>
    public void SetProfileItems(IEnumerable<ManageProfilesProfileItemViewModel> profileItems)
    {
        _profileItems = profileItems?.ToArray() ?? Array.Empty<ManageProfilesProfileItemViewModel>();
    }
}
}
