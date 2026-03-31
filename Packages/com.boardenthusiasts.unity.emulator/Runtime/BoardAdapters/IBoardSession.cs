using System;
using System.Threading.Tasks;

using Board.Core;
using Board.Session;

namespace BoardGDK.BoardAdapters
{
/// <summary>
/// Abstraction over <see cref="BoardSession"/>.
/// </summary>
public interface IBoardSession
{
    /// <summary>
    /// Gets the players currently assigned to the Board session.
    /// </summary>
    BoardSessionPlayer[] Players { get; }
    /// <summary>
    /// Gets the active Board profile.
    /// </summary>
    BoardPlayer ActiveProfile { get; }

    /// <inheritdoc cref="BoardSession.playersChanged"/>
    event Action PlayersChanged;
    /// <inheritdoc cref="BoardSession.activeProfileChanged"/>
    event Action ActiveProfileChanged;

    /// <inheritdoc cref="BoardSession.PresentAddPlayerSelector"/>
    Task<bool> PresentAddPlayerSelector();
    /// <inheritdoc cref="BoardSession.ResetPlayers"/>
    bool ResetPlayers();
    /// <inheritdoc cref="BoardSession.PresentReplacePlayerSelector"/>
    Task<bool> PresentReplacePlayerSelector(BoardSessionPlayer player);
}
}
