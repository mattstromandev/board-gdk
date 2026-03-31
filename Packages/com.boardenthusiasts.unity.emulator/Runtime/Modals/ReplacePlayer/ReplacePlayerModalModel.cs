using System;
using System.Collections.Generic;
using System.Linq;

using BE.Emulator.Framework;
using BE.Emulator.Modals.AddPlayer;

namespace BE.Emulator.Modals.ReplacePlayer
{
/// <summary>
/// Backing model for the replace-player modal.
/// </summary>
internal sealed class ReplacePlayerModalModel : IModel
{
    private IReadOnlyList<AddPlayerCardViewModel> _replacementCards = Array.Empty<AddPlayerCardViewModel>();

    /// <summary>
    /// The session identifier currently being targeted by the modal.
    /// </summary>
    public int TargetSessionId { get; private set; } = -1;

    /// <summary>
    /// The player currently being removed or replaced.
    /// </summary>
    public AddPlayerCardViewModel CurrentPlayer { get; private set; }

    /// <summary>
    /// Whether the current player can be removed from the session.
    /// </summary>
    public bool CanRemoveCurrentPlayer { get; private set; }

    /// <summary>
    /// The candidate replacement profile cards.
    /// </summary>
    public IReadOnlyList<AddPlayerCardViewModel> ReplacementCards => _replacementCards;

    /// <summary>
    /// Replaces the modal state with the provided target player and replacement cards.
    /// </summary>
    public void SetState(int targetSessionId, AddPlayerCardViewModel currentPlayer, bool canRemoveCurrentPlayer, IEnumerable<AddPlayerCardViewModel> replacementCards)
    {
        TargetSessionId = targetSessionId;
        CurrentPlayer = currentPlayer;
        CanRemoveCurrentPlayer = canRemoveCurrentPlayer;
        _replacementCards = replacementCards?.ToArray() ?? Array.Empty<AddPlayerCardViewModel>();
    }
}
}
