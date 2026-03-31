using System;

using Board.Core;

using BoardGDK.BoardAdapters;

using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Zenject;

namespace BE.Emulator.Services
{
/// <summary>
/// Runtime service that exposes the application-facing Board facade through either the emulator model or live SDK API.
/// </summary>
internal sealed class BoardApplicationService : IBoardApplication, IDisposable
{
    private readonly IEmulatorModel _model;

    /// <summary>
    /// Creates the application service.
    /// </summary>
    /// <param name="model">Optional editor-only emulator model backing the service.</param>
    public BoardApplicationService([InjectOptional] [CanBeNull] IEmulatorModel model = null)
    {
        _model = model;

#if UNITY_EDITOR
        if(_model != null)
        {
            return;
        }
#endif

        BoardApplication.customPauseScreenButtonPressed += OnCustomPauseScreenButtonPressed;
        BoardApplication.pauseScreenActionReceived += OnPauseScreenActionReceived;
    }

    /// <inheritdoc />
    public bool IsProfileSwitcherVisible
    {
        get
        {
#if UNITY_EDITOR
            if(_model != null)
            {
                return _model.IsProfileSwitcherVisible;
            }
#endif
            return false;
        }
    }

    /// <inheritdoc />
    public BoardPauseScreenContext CurrentPauseScreenContext
    {
        get
        {
#if UNITY_EDITOR
            if(_model != null)
            {
                return _model.CurrentPauseScreenContext;
            }
#endif
            return new BoardPauseScreenContext();
        }
    }

    /// <inheritdoc />
    public event PauseScreenCustomButtonPressedHandler CustomPauseScreenButtonPressed;
    /// <inheritdoc />
    public event PauseScreenActionReceivedHandler PauseScreenActionReceived;

    /// <inheritdoc />
    public void Dispose()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return;
        }
#endif

        BoardApplication.customPauseScreenButtonPressed -= OnCustomPauseScreenButtonPressed;
        BoardApplication.pauseScreenActionReceived -= OnPauseScreenActionReceived;
    }

    /// <inheritdoc />
    public void ShowProfileSwitcher()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            _model.ShowProfileSwitcher();
            return;
        }
#endif
        BoardApplication.ShowProfileSwitcher();
    }

    /// <inheritdoc />
    public void HideProfileSwitcher()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            _model.HideProfileSwitcher();
            return;
        }
#endif
        BoardApplication.HideProfileSwitcher();
    }

    /// <inheritdoc />
    public void SetPauseScreenContext(BoardPauseScreenContext context)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            _model.SetPauseScreenContext(context);
            return;
        }
#endif
        BoardApplication.SetPauseScreenContext(context);
    }

    /// <inheritdoc />
    public void SetPauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        BoardPauseCustomButton[] customButtons = null,
        BoardPauseAudioTrack[] audioTracks = null)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            _model.SetPauseScreenContext(applicationName, showSaveOptionUponExit, customButtons, audioTracks);
            return;
        }
#endif
        BoardApplication.SetPauseScreenContext(applicationName, showSaveOptionUponExit, customButtons, audioTracks);
    }

    /// <inheritdoc />
    public void UpdatePauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        BoardPauseCustomButton[] customButtons = null,
        BoardPauseAudioTrack[] audioTracks = null)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            _model.UpdatePauseScreenContext(applicationName, showSaveOptionUponExit, customButtons, audioTracks);
            return;
        }
#endif
        BoardApplication.UpdatePauseScreenContext(applicationName, showSaveOptionUponExit, customButtons, audioTracks);
    }

    /// <inheritdoc />
    public void ClearPauseScreenContext()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            _model.ClearPauseScreenContext();
            return;
        }
#endif
        BoardApplication.ClearPauseScreenContext();
    }

    /// <inheritdoc />
    public void Exit()
    {
        BoardApplication.Exit();
    }

    private void OnCustomPauseScreenButtonPressed(string customButtonId, BoardPauseAudioTrack[] audioTracks)
    {
        CustomPauseScreenButtonPressed?.Invoke(customButtonId, audioTracks);
    }

    private void OnPauseScreenActionReceived(BoardPauseAction pauseAction, BoardPauseAudioTrack[] audioTracks)
    {
        PauseScreenActionReceived?.Invoke(pauseAction, audioTracks);
    }
}
}
