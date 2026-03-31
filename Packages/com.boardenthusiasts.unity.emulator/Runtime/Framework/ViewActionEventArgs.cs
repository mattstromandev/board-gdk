using System;

using JetBrains.Annotations;

namespace BE.Emulator.Framework
{
/// <summary>
/// Event arguments carrying an <see cref="IViewAction"/> published by an <see cref="IView{T}"/>.
/// </summary>
public sealed class ViewActionEventArgs : EventArgs
{
    /// <summary>
    /// The action published by the view.
    /// </summary>
    public IViewAction Action { get; }

    /// <summary>
    /// Creates a new set of event arguments for the provided <paramref name="action"/>.
    /// </summary>
    /// <param name="action">The published view action.</param>
    public ViewActionEventArgs([NotNull] IViewAction action)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
}
}
