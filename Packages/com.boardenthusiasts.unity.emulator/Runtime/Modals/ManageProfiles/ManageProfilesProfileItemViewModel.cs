using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Unity.Properties;

using UnityEngine.UIElements;

// ReSharper disable InconsistentNaming : event name must match Unity.Properties.INotifyBindablePropertyChanged.

namespace BE.Emulator.Modals.ManageProfiles
{
/// <summary>
/// Bindable view model representing a single profile item in the manage profiles modal.
/// </summary>
[Serializable]
internal sealed class ManageProfilesProfileItemViewModel : INotifyBindablePropertyChanged
{
    private string _playerId = string.Empty;
    private string _displayName = string.Empty;
    private StyleBackground _avatarImage;
    private StyleColor _avatarBackgroundColor;

    /// <summary>
    /// The Board player identifier represented by this list item.
    /// </summary>
    public string PlayerId
    {
        get => _playerId;
        set => _playerId = value ?? string.Empty;
    }

    /// <summary>
    /// The display name shown by this list item.
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
    /// The avatar background shown by this list item.
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
    /// The avatar background color shown by this list item.
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

    /// <inheritdoc />
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    private void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
}
