namespace BE.Emulator.Framework
{
/// <summary>
/// The display lifecycle operation that should be invoked for a routed <see cref="IViewAction"/>.
/// </summary>
public enum EmulatorDisplayOperation
{
    /// <summary>
    /// Attaches the target display to its resolved host.
    /// </summary>
    Attach,

    /// <summary>
    /// Shows the target display.
    /// </summary>
    Show,

    /// <summary>
    /// Hides the target display.
    /// </summary>
    Hide,

    /// <summary>
    /// Detaches the target display from its current host.
    /// </summary>
    Detach
}
}
