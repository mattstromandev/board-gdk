using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Unity.Properties;

using UnityEngine.UIElements;

using BE.Emulator.Framework;

// ReSharper disable InconsistentNaming : event name must match Unity.Properties.INotifyBindablePropertyChanged.

namespace BE.Emulator
{
/// <summary>
/// View model for the <see cref="Emulator"/>.
/// </summary>
[Serializable]
internal sealed class EmulatorViewModel : IViewModel, INotifyBindablePropertyChanged
{
    private string _username = string.Empty;
    private StyleBackground _avatarImage;
    private StyleColor _avatarBackgroundColor;

    /// <summary>
    /// The <see cref="PanelSettings"/> that should drive the emulator document.
    /// </summary>
    public PanelSettings PanelSettings { get; set; }

    /// <summary>
    /// The sorting order that should be applied to the emulator document.
    /// </summary>
    public float SortOrder { get; set; }

    /// <summary>
    /// The shell template to clone into the emulator document.
    /// </summary>
    public VisualTreeAsset SourceTemplate { get; set; }

    /// <summary>
    /// The username for the selected profile.
    /// </summary>
    [CreateProperty]
    public string Username
    {
        get => _username;
        set
        {
            if(string.Equals(_username, value, StringComparison.Ordinal))
            {
                return;
            }

            _username = value ?? string.Empty;
            Notify();
        }
    }

    /// <summary>
    /// The avatar background for the selected profile.
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
    /// The avatar background color for the selected profile.
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
