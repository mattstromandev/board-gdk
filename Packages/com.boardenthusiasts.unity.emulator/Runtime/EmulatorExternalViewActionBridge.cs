using System;

using BE.Emulator.Actions;
using BE.Emulator.Framework;

namespace BE.Emulator
{
/// <summary>
/// Runtime bridge that allows editor tooling to publish emulator view actions during play mode.
/// </summary>
public static class EmulatorExternalViewActionBridge
{
    /// <summary>
    /// Raised when external tooling requests a view action to be routed through the running emulator shell.
    /// </summary>
    public static event EventHandler<ViewActionEventArgs> ViewActionRequested;

    /// <summary>
    /// The last requested safe-space visibility state.
    /// </summary>
    public static bool IsSafeSpaceVisible { get; private set; }

    /// <summary>
    /// Publish a view action into the running emulator shell.
    /// </summary>
    /// <param name="action">The action to route through the emulator shell.</param>
    public static void Request(IViewAction action)
    {
        if(action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if(action is SetSafeSpaceVisibilityViewAction safeSpaceAction)
        {
            IsSafeSpaceVisible = safeSpaceAction.IsVisible;
        }

        ViewActionRequested?.Invoke(null, new ViewActionEventArgs(action));
    }
}
}
