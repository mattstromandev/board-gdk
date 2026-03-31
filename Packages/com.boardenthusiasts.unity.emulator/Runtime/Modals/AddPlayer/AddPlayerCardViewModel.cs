using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Board.Core;

using Unity.Properties;

using UnityEngine.UIElements;

// ReSharper disable InconsistentNaming : event name must match Unity.Properties.INotifyBindablePropertyChanged.

namespace BE.Emulator.Modals.AddPlayer
{
/// <summary>
/// Bindable view model for a selectable add-player card.
/// </summary>
[Serializable]
internal sealed class AddPlayerCardViewModel : INotifyBindablePropertyChanged
{
    private string _playerId = string.Empty;
    private string _displayName = string.Empty;
    private StyleBackground _avatarImage;
    private StyleColor _avatarBackgroundColor;
    private BoardPlayerType _playerType;

    /// <summary>
    /// The selected persistent profile identifier when this card represents a profile.
    /// </summary>
    public string PlayerId
    {
        get => _playerId;
        set => _playerId = value ?? string.Empty;
    }

    /// <summary>
    /// The display name shown below the card avatar.
    /// </summary>
    [CreateProperty]
    public string DisplayName
    {
        get => _displayName;
        set
        {
            if(string.Equals(_displayName, value, StringComparison.Ordinal))
            {
                return;
            }

            _displayName = value ?? string.Empty;
            Notify();
        }
    }

    /// <summary>
    /// The avatar image displayed for the card.
    /// </summary>
    [CreateProperty]
    public StyleBackground AvatarImage
    {
        get => _avatarImage;
        set
        {
            if(EqualityComparer<StyleBackground>.Default.Equals(_avatarImage, value))
            {
                return;
            }

            _avatarImage = value;
            Notify();
        }
    }

    /// <summary>
    /// The avatar background color displayed for the card.
    /// </summary>
    [CreateProperty]
    public StyleColor AvatarBackgroundColor
    {
        get => _avatarBackgroundColor;
        set
        {
            if(EqualityComparer<StyleColor>.Default.Equals(_avatarBackgroundColor, value))
            {
                return;
            }

            _avatarBackgroundColor = value;
            Notify();
        }
    }

    /// <summary>
    /// The type of player this card adds to the session.
    /// </summary>
    public BoardPlayerType PlayerType
    {
        get => _playerType;
        set => _playerType = value;
    }

    /// <summary>
    /// Whether this card represents the guest option.
    /// </summary>
    public bool IsGuest => PlayerType == BoardPlayerType.Guest;

    /// <inheritdoc />
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    private void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
}
