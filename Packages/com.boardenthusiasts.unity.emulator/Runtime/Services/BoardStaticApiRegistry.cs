using BoardGDK.BoardAdapters;

using JetBrains.Annotations;

namespace BE.Emulator.Services
{
/// <summary>
/// Stores the active runtime implementations backing the emulator's static facade APIs.
/// </summary>
internal static class BoardStaticApiRegistry
{
    private static IBoardApplication s_application;
    private static IBoardSession s_session;
    private static IBoardSaveGameManager s_saveGameManager;

    /// <summary>
    /// Gets the current application-facing Board service.
    /// </summary>
    public static IBoardApplication Application => s_application ?? throw CreateNotReadyException();
    /// <summary>
    /// Gets the current session-facing Board service.
    /// </summary>
    public static IBoardSession Session => s_session ?? throw CreateNotReadyException();
    /// <summary>
    /// Gets the current save-game-facing Board service.
    /// </summary>
    public static IBoardSaveGameManager SaveGameManager => s_saveGameManager ?? throw CreateNotReadyException();

    /// <summary>
    /// Sets the active runtime services used by the static facade APIs.
    /// </summary>
    public static void Set(
        [NotNull] IBoardApplication application,
        [NotNull] IBoardSession session,
        [NotNull] IBoardSaveGameManager saveGameManager)
    {
        s_application = application;
        s_session = session;
        s_saveGameManager = saveGameManager;
    }

    /// <summary>
    /// Clears the active runtime services used by the static facade APIs.
    /// </summary>
    public static void Clear()
    {
        s_application = null;
        s_session = null;
        s_saveGameManager = null;
    }

    private static EmulatorNotReadyException CreateNotReadyException()
    {
        return new EmulatorNotReadyException(
            "The BE Emulator for Board static API was accessed before the emulator bindings were initialized. " +
            "Install the BE Emulator for Board and wait until its container has finished initializing before calling the static Board facade.");
    }
}
}
