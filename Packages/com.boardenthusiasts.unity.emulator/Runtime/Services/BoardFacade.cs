using BoardGDK.BoardAdapters;

using JetBrains.Annotations;

namespace BE.Emulator.Services
{
/// <summary>
/// Aggregates the emulator's application, session, and save-game services behind the root Board facade abstraction.
/// </summary>
internal sealed class BoardFacade : IBoard
{
    /// <summary>
    /// Creates the facade.
    /// </summary>
    /// <param name="application">The application-facing Board service.</param>
    /// <param name="session">The session-facing Board service.</param>
    /// <param name="saveGameManager">The save-game-facing Board service.</param>
    public BoardFacade(
        [NotNull] IBoardApplication application,
        [NotNull] IBoardSession session,
        [NotNull] IBoardSaveGameManager saveGameManager)
    {
        Application = application;
        Session = session;
        SaveGameManager = saveGameManager;
    }

    /// <inheritdoc />
    public IBoardApplication Application { get; }
    /// <inheritdoc />
    public IBoardSession Session { get; }
    /// <inheritdoc />
    public IBoardSaveGameManager SaveGameManager { get; }
}
}
