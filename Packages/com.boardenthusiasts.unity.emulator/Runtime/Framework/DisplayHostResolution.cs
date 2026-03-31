using System;

using JetBrains.Annotations;

using UnityEngine.UIElements;

namespace BE.Emulator.Framework
{
/// <summary>
/// The resolved host information for attaching a display.
/// </summary>
public sealed class DisplayHostResolution
{
    /// <summary>
    /// The <see cref="VisualElement"/> that should contain the target display.
    /// </summary>
    public VisualElement Host { get; }

    /// <summary>
    /// Whether the resolved host should be cleared before the display is attached.
    /// </summary>
    public bool ClearHostOnAttach { get; }

    /// <summary>
    /// Creates a new host resolution.
    /// </summary>
    /// <param name="host">The resolved host element.</param>
    /// <param name="clearHostOnAttach">Whether the host should be cleared before attaching the display.</param>
    public DisplayHostResolution([NotNull] VisualElement host, bool clearHostOnAttach)
    {
        Host = host ?? throw new ArgumentNullException(nameof(host));
        ClearHostOnAttach = clearHostOnAttach;
    }
}
}
