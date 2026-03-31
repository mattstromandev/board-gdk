using System;
using System.Linq;

using Board.Core;

using BE.Emulator.Actions;
using BE.Emulator.Data;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine.UIElements;

using Zenject;

namespace BE.Emulator.Modals.AddPlayer
{
/// <summary>
/// Controller for the add-player modal.
/// </summary>
internal sealed class AddPlayerModal
    : DisplayableController<AddPlayerModalModel, AddPlayerModalView, AddPlayerModalViewModel>, IEmulatorModal, IViewActionHandler
{
    /// <summary>
    /// The logical display name for the add-player modal.
    /// </summary>
    public const string DisplayName = "add-player";

    /// <inheritdoc />
    public string Name => DisplayName;

    private readonly AddPlayerModalModel _model;
    private readonly AddPlayerModalView _view;
    private readonly IEmulatorModel _emulatorModel;

    /// <summary>
    /// Creates the add-player modal controller.
    /// </summary>
    public AddPlayerModal(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] AddPlayerModalModel model,
        [NotNull] AddPlayerModalView view,
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
        RefreshCards();

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
        if(action is not PresentAddPlayerSelectorViewAction)
        {
            return;
        }

        RefreshCards();
    }

    private void OnEmulatorModelChanged(object sender, EventArgs eventArgs)
    {
        RefreshCards();
    }

    private void RefreshCards()
    {
        EmulatorMockData currentData = _emulatorModel?.CurrentData;
        var activePlayerIds = currentData?.Session?.Players?
            .Select(player => player?.PlayerId)
            .Where(playerId => string.IsNullOrWhiteSpace(playerId) == false)
            .ToHashSet(StringComparer.Ordinal)
            ?? new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

        AddPlayerCardViewModel[] cards = currentData?.Profiles?
            .Where(profile => profile != null && activePlayerIds.Contains(profile.PlayerId) == false)
            .Select(CreateProfileCard)
            .Concat(new[] { CreateGuestCard() })
            .ToArray()
            ?? new[] { CreateGuestCard() };

        _model.SetCards(cards);
        _view.SetCards(_model.Cards);
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

    private static AddPlayerCardViewModel CreateGuestCard()
    {
        return new AddPlayerCardViewModel
        {
            DisplayName = "Guest",
            PlayerType = BoardPlayerType.Guest
        };
    }
}
}
