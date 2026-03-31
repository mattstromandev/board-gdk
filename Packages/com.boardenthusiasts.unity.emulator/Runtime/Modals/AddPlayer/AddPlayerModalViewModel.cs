using System;
using System.Collections.Generic;

using BE.Emulator.Framework;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.AddPlayer
{
/// <summary>
/// View model for the add-player modal.
/// </summary>
[Serializable]
internal sealed class AddPlayerModalViewModel : IViewModel
{
    private readonly List<AddPlayerCardViewModel> _cards = new();

    /// <summary>
    /// The template to clone when constructing the add-player modal view tree.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }

    /// <summary>
    /// The template to clone when constructing each add-player card.
    /// </summary>
    public VisualTreeAsset CardTemplate { get; set; }

    /// <summary>
    /// The cards that should be rendered by the add-player modal.
    /// </summary>
    public IReadOnlyList<AddPlayerCardViewModel> Cards => _cards;

    /// <summary>
    /// Replace the currently rendered cards.
    /// </summary>
    public void SetCards(IEnumerable<AddPlayerCardViewModel> cards)
    {
        _cards.Clear();

        if(cards == null)
        {
            return;
        }

        _cards.AddRange(cards);
    }
}
}
