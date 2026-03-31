using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Unity.Properties;

using UnityEngine.UIElements;

// ReSharper disable InconsistentNaming : event name must match Unity.Properties.INotifyBindablePropertyChanged.

namespace BE.Emulator.Screens.ProfileSwitcher
{
/// <summary>
/// Bindable view model representing a single profile card in the profile switcher.
/// </summary>
[Serializable]
internal sealed class ProfileCardViewModel : INotifyBindablePropertyChanged
{
    private string _playerId = string.Empty;
    private string _displayName = string.Empty;
    private StyleBackground _avatarImage;
    private StyleColor _avatarBackgroundColor;
    private bool _isActive;

    /// <summary>
    /// The Board player identifier represented by this card.
    /// </summary>
    public string PlayerId
    {
        get => _playerId;
        set => _playerId = value ?? string.Empty;
    }

    /// <summary>
    /// The display name for the profile card.
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
    /// The avatar background shown for the profile card.
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
    /// The avatar background color shown for the profile card.
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
    /// Whether the profile card represents the currently active profile.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value;
    }

    /// <inheritdoc />
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    private void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
}
