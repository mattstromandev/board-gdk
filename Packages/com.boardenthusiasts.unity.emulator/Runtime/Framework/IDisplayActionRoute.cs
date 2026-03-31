using System;

namespace BE.Emulator.Framework
{
/// <summary>
/// Describes how a routed <see cref="IViewAction"/> should affect a target <see cref="IEmulatorDisplay"/>.
/// </summary>
public interface IDisplayActionRoute
{
    /// <summary>
    /// The action type that triggers this route.
    /// </summary>
    public Type ActionType { get; }

    /// <summary>
    /// The target display type that should receive the configured operation.
    /// </summary>
    public Type TargetDisplayType { get; }

    /// <summary>
    /// The display operation that should be invoked.
    /// </summary>
    public EmulatorDisplayOperation Operation { get; }
}

/// <summary>
/// Describes which handler should receive a routed <see cref="IViewAction"/>.
/// </summary>
public interface IViewActionHandlerRoute
{
    /// <summary>
    /// The action type that triggers this route.
    /// </summary>
    public Type ActionType { get; }

    /// <summary>
    /// The handler type that should receive the routed action.
    /// </summary>
    public Type TargetHandlerType { get; }
}
}
