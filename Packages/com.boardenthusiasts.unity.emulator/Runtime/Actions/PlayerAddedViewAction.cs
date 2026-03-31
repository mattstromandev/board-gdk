using Board.Core;

using BE.Emulator.Framework;

namespace BE.Emulator.Actions
{
/// <summary>
/// Requests that the selected player be added to the current session.
/// </summary>
internal sealed class PlayerAddedViewAction : IViewAction
{
    /// <summary>
    /// Gets or sets the selected persistent profile identifier when adding a profile player.
    /// </summary>
    public string SelectedProfileId { get; set; }

    /// <summary>
    /// Gets or sets the type of player that should be added.
    /// </summary>
    public BoardPlayerType PlayerType { get; set; }
}
}
