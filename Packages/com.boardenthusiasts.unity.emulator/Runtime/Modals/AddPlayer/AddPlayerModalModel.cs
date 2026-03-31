using System;
using System.Collections.Generic;
using System.Linq;

using BE.Emulator.Framework;

namespace BE.Emulator.Modals.AddPlayer
{
/// <summary>
/// Backing model for the add-player modal.
/// </summary>
internal sealed class AddPlayerModalModel : IModel
{
    private IReadOnlyList<AddPlayerCardViewModel> _cards = Array.Empty<AddPlayerCardViewModel>();

    /// <summary>
    /// The cards currently represented by the modal.
    /// </summary>
    public IReadOnlyList<AddPlayerCardViewModel> Cards => _cards;

    /// <summary>
    /// Replace the current cards with the provided set.
    /// </summary>
    public void SetCards(IEnumerable<AddPlayerCardViewModel> cards)
    {
        _cards = cards?.ToArray() ?? Array.Empty<AddPlayerCardViewModel>();
    }
}
}
