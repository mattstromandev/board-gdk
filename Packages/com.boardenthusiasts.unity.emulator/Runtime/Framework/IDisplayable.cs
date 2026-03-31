using System;

using JetBrains.Annotations;

using UnityEngine.UIElements;

namespace BE.Emulator.Framework
{
/// <summary>
/// Interface for an entity which can be displayed using Unity's UI Toolkit.
/// </summary>
public interface IDisplayable
{
    /// <summary>
    /// Raised when this <see cref="IDisplayable"/> is initialized and ready for attachment.
    /// </summary>
    public event EventHandler ReadyForAttachment;
    
    /// <summary>
    /// Flag indicating if this <see cref="IDisplayable"/> is ready for attachment.
    /// </summary>
    public bool IsReady { get; }
    
    /// <summary>
    /// Flag indicating if this <see cref="IDisplayable"/> is currently attached to a host.
    /// </summary>
    public bool IsAttached { get; }
    
    /// <summary>
    /// Flag indicating if this <see cref="IDisplayable"/> is currently displayed.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// Attach this <see cref="IDisplayable"/> to the given <see cref="VisualElement"/> as a host container.
    /// </summary>
    /// <param name="host">The <see cref="VisualElement"/> which will contain this <see cref="IDisplayable"/>.</param>
    /// <param name="clearHost">Flag indicating whether the <paramref name="host"/> should be cleared when attaching.</param>
    /// <param name="hideAfterAttach">Flag indicating whether this <see cref="IDisplayable"/> should be hidden after attaching.</param>
    public void Attach([NotNull] VisualElement host, bool clearHost = true, bool hideAfterAttach = true);

    /// <summary>
    /// Show this <see cref="IDisplayable"/>.
    /// </summary>
    public void Show();

    /// <summary>
    /// Hide this <see cref="IDisplayable"/>.
    /// </summary>
    public void Hide();

    /// <summary>
    /// Detach this <see cref="IDisplayable"/> from its host container, if it is attached to one.
    /// </summary>
    public void Detach();
}
}
