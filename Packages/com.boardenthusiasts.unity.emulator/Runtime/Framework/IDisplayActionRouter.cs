using JetBrains.Annotations;

namespace BE.Emulator.Framework
{
/// <summary>
/// Routes published <see cref="IViewAction"/> instances to configured display operations.
/// </summary>
public interface IDisplayActionRouter
{
    /// <summary>
    /// Route the provided <paramref name="action"/> that originated from <paramref name="sender"/>.
    /// </summary>
    /// <param name="sender">The source that published the action.</param>
    /// <param name="action">The action to route.</param>
    public void Route([CanBeNull] object sender, [NotNull] IViewAction action);
}

/// <summary>
/// Handles routed <see cref="IViewAction"/> instances.
/// </summary>
public interface IViewActionHandler
{
    /// <summary>
    /// Handle a routed <paramref name="action"/>.
    /// </summary>
    /// <param name="sender">The source that published the action.</param>
    /// <param name="action">The routed action.</param>
    public void HandleRoutedViewAction([CanBeNull] object sender, [NotNull] IViewAction action);
}
}
