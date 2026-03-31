using System;
using System.Linq;

using Board.Core;

using BE.Emulator.Actions;
using BE.Emulator.Data;
using BE.Emulator.Framework;
using BE.Emulator.Modals.AddPlayer;
using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

using Zenject;

namespace BE.Emulator.Modals.ReplacePlayer
{
/// <summary>
/// Controller for the replace-player modal.
/// </summary>
internal sealed class ReplacePlayerModal
    : DisplayableController<ReplacePlayerModalModel, ReplacePlayerModalView, ReplacePlayerModalViewModel>, IEmulatorModal, IViewActionHandler
{
    /// <summary>
    /// The logical display name for the replace-player modal.
    /// </summary>
    public const string DisplayName = "replace-player";

    /// <inheritdoc />
    public string Name => DisplayName;

    private readonly ReplacePlayerModalModel _model;
    private readonly ReplacePlayerModalView _view;
    private readonly IEmulatorModel _emulatorModel;

    /// <summary>
    /// Creates the replace-player modal controller.
    /// </summary>
    public ReplacePlayerModal(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] ReplacePlayerModalModel model,
        [NotNull] ReplacePlayerModalView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter,
        [InjectOptional] IEmulatorModel emulatorModel = null)
        : base(loggerFactory, model, view, displayActionRouter)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _emulatorModel = emulatorModel;
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        RefreshState();

#if UNITY_EDITOR
        if(_emulatorModel != null)
        {
            _emulatorModel.Changed += OnEmulatorModelChanged;
        }
#endif
    }

    /// <inheritdoc />
    public override void Dispose()
    {
#if UNITY_EDITOR
        if(_emulatorModel != null)
        {
            _emulatorModel.Changed -= OnEmulatorModelChanged;
        }
#endif

        base.Dispose();
    }

    /// <inheritdoc />
    public void HandleRoutedViewAction(object sender, IViewAction action)
    {
        if(action is not PresentReplacePlayerSelectorViewAction replaceAction)
        {
            return;
        }

        RefreshState(replaceAction.TargetSessionId);
    }

    private void OnEmulatorModelChanged(object sender, EventArgs eventArgs)
    {
        RefreshState();
    }

    private void RefreshState(int targetSessionId = int.MinValue)
    {
        int resolvedTargetSessionId = targetSessionId == int.MinValue ? _model.TargetSessionId : targetSessionId;
        EmulatorMockData currentData = _emulatorModel?.CurrentData;
        EmulatorSessionPlayerData targetPlayer = currentData?.Session?.Players?
            .FirstOrDefault(player => player?.SessionId == resolvedTargetSessionId);
        int nonGuestPlayerCount = currentData?.Session?.Players?.Count(player => player?.Type != BoardPlayerType.Guest) ?? 0;

        AddPlayerCardViewModel currentPlayerCard = CreateSessionPlayerCard(currentData, targetPlayer);
        bool canRemoveCurrentPlayer = targetPlayer != null
            && (targetPlayer.Type == BoardPlayerType.Guest || nonGuestPlayerCount > 1);
        AddPlayerCardViewModel[] replacementCards = currentData?.Profiles?
            .Where(profile => profile != null)
            .Where(profile => currentData.Session.Players.Any(sessionPlayer =>
                string.Equals(sessionPlayer?.PlayerId, profile.PlayerId, StringComparison.Ordinal)) == false)
            .Select(CreateProfileCard)
            .ToArray()
            ?? Array.Empty<AddPlayerCardViewModel>();

        _model.SetState(resolvedTargetSessionId, currentPlayerCard, canRemoveCurrentPlayer, replacementCards);
        _view.SetState(_model.TargetSessionId, _model.CurrentPlayer, _model.CanRemoveCurrentPlayer, _model.ReplacementCards);
    }

    private static AddPlayerCardViewModel CreateProfileCard(EmulatorProfileData profile)
    {
        return new AddPlayerCardViewModel
        {
            PlayerId = profile?.PlayerId ?? string.Empty,
            DisplayName = profile?.DisplayName ?? string.Empty,
            AvatarImage = new StyleBackground(profile?.Avatar),
            AvatarBackgroundColor = new StyleColor(profile?.AvatarBackgroundColor ?? default),
            PlayerType = BoardPlayerType.Profile
        };
    }

    private static AddPlayerCardViewModel CreateSessionPlayerCard(EmulatorMockData data, EmulatorSessionPlayerData sessionPlayer)
    {
        if(sessionPlayer == null)
        {
            return null;
        }

        EmulatorProfileData persistedProfile = data?.Profiles?.FirstOrDefault(profile =>
            string.Equals(profile?.PlayerId, sessionPlayer.PlayerId, StringComparison.Ordinal));
        UnityEngine.Color avatarBackgroundColor = persistedProfile != null
            ? persistedProfile.AvatarBackgroundColor
            : sessionPlayer.AvatarBackgroundColor;

        return new AddPlayerCardViewModel
        {
            PlayerId = sessionPlayer.PlayerId ?? string.Empty,
            DisplayName = persistedProfile?.DisplayName ?? sessionPlayer.DisplayName ?? string.Empty,
            AvatarImage = new StyleBackground(persistedProfile?.Avatar ?? sessionPlayer.Avatar),
            AvatarBackgroundColor = new StyleColor(avatarBackgroundColor),
            PlayerType = sessionPlayer.Type
        };
    }
}
}
