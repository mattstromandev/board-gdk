using System;

using JetBrains.Annotations;

namespace BE.Emulator.Framework
{
/// <inheritdoc cref="IDisplayActionRoute"/>
public sealed class DisplayActionRoute : IDisplayActionRoute
{
    /// <inheritdoc />
    public Type ActionType { get; }

    /// <inheritdoc />
    public Type TargetDisplayType { get; }

    /// <inheritdoc />
    public EmulatorDisplayOperation Operation { get; }

    /// <summary>
    /// Creates a new route definition.
    /// </summary>
    /// <param name="actionType">The action type that should trigger the route.</param>
    /// <param name="targetDisplayType">The target display type that should receive the route.</param>
    /// <param name="operation">The operation that should be performed on the target display.</param>
    public DisplayActionRoute([NotNull] Type actionType, [NotNull] Type targetDisplayType, EmulatorDisplayOperation operation)
    {
        ActionType = actionType ?? throw new ArgumentNullException(nameof(actionType));
        TargetDisplayType = targetDisplayType ?? throw new ArgumentNullException(nameof(targetDisplayType));
        Operation = operation;
    }

    /// <summary>
    /// Creates a strongly typed route definition.
    /// </summary>
    /// <typeparam name="TAction">The action type that should trigger the route.</typeparam>
    /// <typeparam name="TDisplay">The target display type that should receive the route.</typeparam>
    /// <param name="operation">The operation that should be performed on the target display.</param>
    /// <returns>The created route definition.</returns>
    public static DisplayActionRoute Create<TAction, TDisplay>(EmulatorDisplayOperation operation)
        where TAction : IViewAction where TDisplay : IEmulatorDisplay
    {
        return new DisplayActionRoute(typeof(TAction), typeof(TDisplay), operation);
    }
}

/// <inheritdoc cref="IViewActionHandlerRoute"/>
public sealed class ViewActionHandlerRoute : IViewActionHandlerRoute
{
    /// <inheritdoc />
    public Type ActionType { get; }

    /// <inheritdoc />
    public Type TargetHandlerType { get; }

    /// <summary>
    /// Creates a new route definition.
    /// </summary>
    /// <param name="actionType">The action type that should trigger the route.</param>
    /// <param name="targetHandlerType">The handler type that should receive the action.</param>
    public ViewActionHandlerRoute([NotNull] Type actionType, [NotNull] Type targetHandlerType)
    {
        ActionType = actionType ?? throw new ArgumentNullException(nameof(actionType));
        TargetHandlerType = targetHandlerType ?? throw new ArgumentNullException(nameof(targetHandlerType));
    }

    /// <summary>
    /// Creates a strongly typed route definition.
    /// </summary>
    public static ViewActionHandlerRoute Create<TAction, THandler>()
        where TAction : IViewAction
        where THandler : IViewActionHandler
    {
        return new ViewActionHandlerRoute(typeof(TAction), typeof(THandler));
    }
}
}
