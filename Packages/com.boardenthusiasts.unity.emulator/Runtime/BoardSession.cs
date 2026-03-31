using System;
using System.Threading.Tasks;

using BE.Emulator.Services;

// ReSharper disable InconsistentNaming : names must match Board SDK exactly to ensure correct API bridging.

namespace BE.Emulator
{
/// <summary>
/// Static facade over the emulator's session-facing Board surface.
/// </summary>
public static class BoardSession
{
    /// <inheritdoc cref="global::Board.Session.BoardSession.players"/>
    public static Board.Session.BoardSessionPlayer[] players => BoardStaticApiRegistry.Session.Players;

    /// <inheritdoc cref="global::Board.Session.BoardSession.playersChanged"/>
    public static event Action playersChanged
    {
        add => BoardStaticApiRegistry.Session.PlayersChanged += value;
        remove => BoardStaticApiRegistry.Session.PlayersChanged -= value;
    }

    /// <inheritdoc cref="global::Board.Session.BoardSession.activeProfile"/>
    public static Board.Core.BoardPlayer activeProfile => BoardStaticApiRegistry.Session.ActiveProfile;

    /// <inheritdoc cref="global::Board.Session.BoardSession.activeProfileChanged"/>
    public static event Action activeProfileChanged
    {
        add => BoardStaticApiRegistry.Session.ActiveProfileChanged += value;
        remove => BoardStaticApiRegistry.Session.ActiveProfileChanged -= value;
    }

    /// <inheritdoc cref="global::Board.Session.BoardSession.PresentAddPlayerSelector"/>
    public static Task<bool> PresentAddPlayerSelector()
    {
        return BoardStaticApiRegistry.Session.PresentAddPlayerSelector();
    }

    /// <inheritdoc cref="global::Board.Session.BoardSession.ResetPlayers"/>
    public static bool ResetPlayers()
    {
        return BoardStaticApiRegistry.Session.ResetPlayers();
    }

    /// <inheritdoc cref="global::Board.Session.BoardSession.PresentReplacePlayerSelector"/>
    public static Task<bool> PresentReplacePlayerSelector(Board.Session.BoardSessionPlayer player)
    {
        return BoardStaticApiRegistry.Session.PresentReplacePlayerSelector(player);
    }
}
}
