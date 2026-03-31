using System;
using System.Collections.Generic;

using BE.Emulator.Framework;

using UnityEngine.UIElements;

namespace BE.Emulator.Screens.ProfileSwitcher
{
/// <summary>
/// View model for the profile switcher screen.
/// </summary>
[Serializable]
internal sealed class ProfileSwitcherViewModel : IViewModel
{
    private readonly List<ProfileCardViewModel> _profileCards = new();

    /// <summary>
    /// The template to clone when constructing the profile switcher view tree.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }

    /// <summary>
    /// The template to clone when constructing each profile card.
    /// </summary>
    public VisualTreeAsset ProfileCardTemplate { get; set; }

    /// <summary>
    /// The profile cards that should be rendered by the profile switcher.
    /// </summary>
    public IReadOnlyList<ProfileCardViewModel> ProfileCards => _profileCards;

    /// <summary>
    /// Replace the currently rendered profile cards.
    /// </summary>
    public void SetProfileCards(IEnumerable<ProfileCardViewModel> profileCards)
    {
        _profileCards.Clear();

        if(profileCards == null)
        {
            return;
        }

        _profileCards.AddRange(profileCards);
    }
}
}
