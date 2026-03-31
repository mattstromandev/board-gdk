using JetBrains.Annotations;

namespace BoardGDK.BoardAdapters
{
/// <summary>
/// Root abstraction over the public Board SDK OS-facing surface.
/// </summary>
public interface IBoard
{
    /// <summary>
    /// Gets the application-facing Board surface.
    /// </summary>
    [NotNull] IBoardApplication Application { get; }

    /// <summary>
    /// Gets the session-facing Board surface.
    /// </summary>
    [NotNull] IBoardSession Session { get; }

    /// <summary>
    /// Gets the save-game-facing Board surface.
    /// </summary>
    [NotNull] IBoardSaveGameManager SaveGameManager { get; }
}
}
