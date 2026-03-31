using System;
using System.Collections.Generic;
using System.Linq;

using BE.Emulator.Framework;

namespace BE.Emulator.Screens.ProfileSwitcher
{
/// <summary>
/// Backing model for the profile switcher display.
/// </summary>
internal sealed class ProfileSwitcherModel : IModel
{
    private IReadOnlyList<ProfileCardViewModel> _profileCards = Array.Empty<ProfileCardViewModel>();

    /// <summary>
    /// The profile cards currently represented by the screen.
    /// </summary>
    public IReadOnlyList<ProfileCardViewModel> ProfileCards => _profileCards;

    /// <summary>
    /// Replace the current profile cards with the provided set.
    /// </summary>
    public void SetProfileCards(IEnumerable<ProfileCardViewModel> profileCards)
    {
        _profileCards = profileCards?.ToArray() ?? Array.Empty<ProfileCardViewModel>();
    }
}
}
