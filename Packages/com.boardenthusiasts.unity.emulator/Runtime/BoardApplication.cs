using BE.Emulator.Services;

// ReSharper disable InconsistentNaming : names must match Board SDK exactly to ensure correct API bridging.

namespace BE.Emulator
{
/// <summary>
/// Static facade over the emulator's application-facing Board surface.
/// </summary>
public static class BoardApplication
{
    /// <inheritdoc cref="global::Board.Core.BoardApplication.customPauseScreenButtonPressed"/>
    public static event Board.Core.PauseScreenCustomButtonPressedHandler customPauseScreenButtonPressed
    {
        add => BoardStaticApiRegistry.Application.CustomPauseScreenButtonPressed += value;
        remove => BoardStaticApiRegistry.Application.CustomPauseScreenButtonPressed -= value;
    }

    /// <inheritdoc cref="global::Board.Core.BoardApplication.pauseScreenActionReceived"/>
    public static event Board.Core.PauseScreenActionReceivedHandler pauseScreenActionReceived
    {
        add => BoardStaticApiRegistry.Application.PauseScreenActionReceived += value;
        remove => BoardStaticApiRegistry.Application.PauseScreenActionReceived -= value;
    }

    /// <inheritdoc cref="global::Board.Core.BoardApplication.ShowProfileSwitcher"/>
    public static void ShowProfileSwitcher()
    {
        BoardStaticApiRegistry.Application.ShowProfileSwitcher();
    }

    /// <inheritdoc cref="global::Board.Core.BoardApplication.HideProfileSwitcher"/>
    public static void HideProfileSwitcher()
    {
        BoardStaticApiRegistry.Application.HideProfileSwitcher();
    }

    /// <inheritdoc cref="global::Board.Core.BoardApplication.SetPauseScreenContext(global::Board.Core.BoardPauseScreenContext)"/>
    public static void SetPauseScreenContext(Board.Core.BoardPauseScreenContext context)
    {
        BoardStaticApiRegistry.Application.SetPauseScreenContext(context);
    }

    /// <inheritdoc cref="global::Board.Core.BoardApplication.SetPauseScreenContext(string, bool?, global::Board.Core.BoardPauseCustomButton[], global::Board.Core.BoardPauseAudioTrack[])"/>
    public static void SetPauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        Board.Core.BoardPauseCustomButton[] customButtons = null,
        Board.Core.BoardPauseAudioTrack[] audioTracks = null)
    {
        BoardStaticApiRegistry.Application.SetPauseScreenContext(applicationName, showSaveOptionUponExit, customButtons, audioTracks);
    }

    /// <inheritdoc cref="global::Board.Core.BoardApplication.UpdatePauseScreenContext"/>
    public static void UpdatePauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        Board.Core.BoardPauseCustomButton[] customButtons = null,
        Board.Core.BoardPauseAudioTrack[] audioTracks = null)
    {
        BoardStaticApiRegistry.Application.UpdatePauseScreenContext(applicationName, showSaveOptionUponExit, customButtons, audioTracks);
    }

    /// <inheritdoc cref="global::Board.Core.BoardApplication.ClearPauseScreenContext"/>
    public static void ClearPauseScreenContext()
    {
        BoardStaticApiRegistry.Application.ClearPauseScreenContext();
    }

    /// <inheritdoc cref="global::Board.Core.BoardApplication.Exit"/>
    public static void Exit()
    {
        BoardStaticApiRegistry.Application.Exit();
    }
}
}
