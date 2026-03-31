using Board.Core;

namespace BoardGDK.BoardAdapters
{
/// <summary>
/// Abstraction over <see cref="BoardApplication"/>.
/// </summary>
public interface IBoardApplication
{
    /// <summary>
    /// Gets whether the profile switcher overlay is currently visible.
    /// </summary>
    bool IsProfileSwitcherVisible { get; }
    /// <summary>
    /// Gets the currently configured pause screen context.
    /// </summary>
    BoardPauseScreenContext CurrentPauseScreenContext { get; }

    /// <inheritdoc cref="BoardApplication.customPauseScreenButtonPressed"/>
    event PauseScreenCustomButtonPressedHandler CustomPauseScreenButtonPressed;
    /// <inheritdoc cref="BoardApplication.pauseScreenActionReceived"/>
    event PauseScreenActionReceivedHandler PauseScreenActionReceived;

    /// <inheritdoc cref="BoardApplication.ShowProfileSwitcher"/>
    void ShowProfileSwitcher();
    /// <inheritdoc cref="BoardApplication.HideProfileSwitcher"/>
    void HideProfileSwitcher();
    /// <inheritdoc cref="BoardApplication.SetPauseScreenContext(BoardPauseScreenContext)"/>
    void SetPauseScreenContext(BoardPauseScreenContext context);
    /// <inheritdoc cref="BoardApplication.SetPauseScreenContext(string, bool?, BoardPauseCustomButton[], BoardPauseAudioTrack[])"/>
    void SetPauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        BoardPauseCustomButton[] customButtons = null,
        BoardPauseAudioTrack[] audioTracks = null);
    /// <inheritdoc cref="BoardApplication.UpdatePauseScreenContext(string, bool?, BoardPauseCustomButton[], BoardPauseAudioTrack[])"/>
    void UpdatePauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        BoardPauseCustomButton[] customButtons = null,
        BoardPauseAudioTrack[] audioTracks = null);
    /// <inheritdoc cref="BoardApplication.ClearPauseScreenContext"/>
    void ClearPauseScreenContext();
    /// <inheritdoc cref="BoardApplication.Exit"/>
    void Exit();
}
}
