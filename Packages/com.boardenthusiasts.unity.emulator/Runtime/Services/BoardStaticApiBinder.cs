using System;

using BoardGDK.BoardAdapters;

using JetBrains.Annotations;

using Zenject;

namespace BE.Emulator.Services
{
/// <summary>
/// Initializes and tears down the static emulator facade bindings for the current container.
/// </summary>
internal sealed class BoardStaticApiBinder : IInitializable, IDisposable
{
    private readonly IBoardApplication _application;
    private readonly IBoardSession _session;
    private readonly IBoardSaveGameManager _saveGameManager;

    /// <summary>
    /// Creates the binder.
    /// </summary>
    /// <param name="application">The application-facing Board service.</param>
    /// <param name="session">The session-facing Board service.</param>
    /// <param name="saveGameManager">The save-game-facing Board service.</param>
    public BoardStaticApiBinder(
        [NotNull] IBoardApplication application,
        [NotNull] IBoardSession session,
        [NotNull] IBoardSaveGameManager saveGameManager)
    {
        _application = application;
        _session = session;
        _saveGameManager = saveGameManager;
    }

    /// <inheritdoc />
    public void Initialize()
    {
        BoardStaticApiRegistry.Set(_application, _session, _saveGameManager);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        BoardStaticApiRegistry.Clear();
    }
}
}
