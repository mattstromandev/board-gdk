using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Board.Core;
using Board.Session;

using BoardGDK.BoardAdapters;

using BE.Emulator.Actions;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Zenject;

namespace BE.Emulator.Services
{
/// <summary>
/// Runtime service that exposes the session-facing Board facade through either the emulator model or live SDK API.
/// </summary>
internal sealed class BoardSessionService : IBoardSession, IViewActionHandler, IDisposable
{
    private readonly IEmulatorModel _model;
    private TaskCompletionSource<bool> _pendingAddPlayerSelector;
    private TaskCompletionSource<bool> _pendingReplacePlayerSelector;
    private int _pendingReplacePlayerSessionId = -1;

    /// <summary>
    /// Creates the session service.
    /// </summary>
    /// <param name="model">Optional editor-only emulator model backing the service.</param>
    public BoardSessionService([InjectOptional] [CanBeNull] IEmulatorModel model = null)
    {
        _model = model;

#if UNITY_EDITOR
        if(_model != null)
        {
            _model.PlayersChanged += OnPlayersChanged;
            _model.ActiveProfileChanged += OnActiveProfileChanged;
            return;
        }
#endif

        BoardSession.playersChanged += OnBoardPlayersChanged;
        BoardSession.activeProfileChanged += OnBoardActiveProfileChanged;
    }

    /// <inheritdoc />
    public BoardSessionPlayer[] Players
    {
        get
        {
#if UNITY_EDITOR
            if(_model != null)
            {
                return _model.Players;
            }
#endif
            return BoardSession.players ?? Array.Empty<BoardSessionPlayer>();
        }
    }

    /// <inheritdoc />
    public BoardPlayer ActiveProfile
    {
        get
        {
#if UNITY_EDITOR
            if(_model != null)
            {
                return _model.ActiveProfile;
            }
#endif
            return BoardSession.activeProfile;
        }
    }

    /// <inheritdoc />
    public event Action PlayersChanged;
    /// <inheritdoc />
    public event Action ActiveProfileChanged;

    /// <inheritdoc />
    public void Dispose()
    {
#if UNITY_EDITOR
        CompletePendingAddPlayerSelector(result: false);
        CompletePendingReplacePlayerSelector(result: false);

        if(_model != null)
        {
            _model.PlayersChanged -= OnPlayersChanged;
            _model.ActiveProfileChanged -= OnActiveProfileChanged;
            return;
        }
#endif

        BoardSession.playersChanged -= OnBoardPlayersChanged;
        BoardSession.activeProfileChanged -= OnBoardActiveProfileChanged;
    }

    /// <inheritdoc />
    public Task<bool> PresentAddPlayerSelector()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            if(HasPendingPlayerSelector())
            {
                throw new InvalidOperationException("Only one player selector can be open at a time.");
            }

            _pendingAddPlayerSelector = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            EmulatorExternalViewActionBridge.Request(new PresentAddPlayerSelectorViewAction());
            return _pendingAddPlayerSelector.Task;
        }
#endif
        return BoardSession.PresentAddPlayerSelector();
    }

    /// <inheritdoc />
    public bool ResetPlayers()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.ResetPlayers();
        }
#endif
        return BoardSession.ResetPlayers();
    }

    /// <inheritdoc />
    public Task<bool> PresentReplacePlayerSelector(BoardSessionPlayer player)
    {
        if(player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

#if UNITY_EDITOR
        if(_model != null)
        {
            if(HasPendingPlayerSelector())
            {
                throw new InvalidOperationException("Only one player selector can be open at a time.");
            }

            if(_model.Players.Any(sessionPlayer => sessionPlayer.sessionId == player.sessionId) == false)
            {
                return Task.FromResult(false);
            }

            _pendingReplacePlayerSelector = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingReplacePlayerSessionId = player.sessionId;
            EmulatorExternalViewActionBridge.Request(new PresentReplacePlayerSelectorViewAction
            {
                TargetSessionId = player.sessionId
            });
            return _pendingReplacePlayerSelector.Task;
        }
#endif
        return BoardSession.PresentReplacePlayerSelector(player);
    }

    /// <inheritdoc />
    public void HandleRoutedViewAction(object sender, IViewAction action)
    {
#if UNITY_EDITOR
        if(_model == null || action == null)
        {
            return;
        }

        switch(action)
        {
            case CloseAddPlayerSelectorViewAction:
                CompletePendingAddPlayerSelector(result: false);
                return;

            case PlayerAddedViewAction playerAddedAction:
                if(_model.AddSessionPlayer(playerAddedAction.SelectedProfileId, playerAddedAction.PlayerType) == false)
                {
                    return;
                }

                CompletePendingAddPlayerSelector(result: true);
                EmulatorExternalViewActionBridge.Request(new CloseAddPlayerSelectorViewAction());
                return;

            case CloseReplacePlayerSelectorViewAction:
                CompletePendingReplacePlayerSelector(result: false);
                return;

            case PlayerRemovedViewAction playerRemovedAction:
                if(_pendingReplacePlayerSelector == null || playerRemovedAction.TargetSessionId != _pendingReplacePlayerSessionId)
                {
                    return;
                }

                if(_model.RemoveSessionPlayer(playerRemovedAction.TargetSessionId) == false)
                {
                    return;
                }

                CompletePendingReplacePlayerSelector(result: true);
                EmulatorExternalViewActionBridge.Request(new CloseReplacePlayerSelectorViewAction());
                return;

            case PlayerReplacedViewAction playerReplacedAction:
                if(_pendingReplacePlayerSelector == null || playerReplacedAction.TargetSessionId != _pendingReplacePlayerSessionId)
                {
                    return;
                }

                if(_model.ReplaceSessionPlayer(playerReplacedAction.TargetSessionId, playerReplacedAction.SelectedProfileId) == false)
                {
                    return;
                }

                CompletePendingReplacePlayerSelector(result: true);
                EmulatorExternalViewActionBridge.Request(new CloseReplacePlayerSelectorViewAction());
                return;
        }
#endif
    }

    private void OnPlayersChanged(object sender, EventArgs eventArgs)
    {
        PlayersChanged?.Invoke();
    }

    private void OnActiveProfileChanged(object sender, EventArgs eventArgs)
    {
        ActiveProfileChanged?.Invoke();
    }

    private void OnBoardPlayersChanged()
    {
        PlayersChanged?.Invoke();
    }

    private void OnBoardActiveProfileChanged()
    {
        ActiveProfileChanged?.Invoke();
    }

#if UNITY_EDITOR
    private void CompletePendingAddPlayerSelector(bool result)
    {
        TaskCompletionSource<bool> pendingSelector = Interlocked.Exchange(ref _pendingAddPlayerSelector, null);
        pendingSelector?.TrySetResult(result);
    }

    private void CompletePendingReplacePlayerSelector(bool result)
    {
        _pendingReplacePlayerSessionId = -1;
        TaskCompletionSource<bool> pendingSelector = Interlocked.Exchange(ref _pendingReplacePlayerSelector, null);
        pendingSelector?.TrySetResult(result);
    }

    private bool HasPendingPlayerSelector()
    {
        return _pendingAddPlayerSelector != null || _pendingReplacePlayerSelector != null;
    }
#endif
}
}
