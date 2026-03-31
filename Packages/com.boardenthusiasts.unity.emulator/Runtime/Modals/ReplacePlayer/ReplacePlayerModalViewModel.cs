using System;
using System.Collections.Generic;

using BE.Emulator.Framework;
using BE.Emulator.Modals.AddPlayer;

using UnityEngine.UIElements;

namespace BE.Emulator.Modals.ReplacePlayer
{
/// <summary>
/// View model for the replace-player modal.
/// </summary>
[Serializable]
internal sealed class ReplacePlayerModalViewModel : IViewModel
{
    private readonly List<AddPlayerCardViewModel> _replacementCards = new();

    /// <summary>
    /// The template used to construct the modal view tree.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }

    /// <summary>
    /// The template used to construct each replacement card.
    /// </summary>
    public VisualTreeAsset CardTemplate { get; set; }

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
    /// The candidate replacement cards currently rendered by the modal.
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

        _replacementCards.Clear();
        if(replacementCards != null)
        {
            _replacementCards.AddRange(replacementCards);
        }
    }
}
}
