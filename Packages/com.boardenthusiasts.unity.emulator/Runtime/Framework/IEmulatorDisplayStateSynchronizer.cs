namespace BE.Emulator.Framework
{
/// <summary>
/// Synchronizes shell-owned visual state after display operations are applied.
/// </summary>
public interface IEmulatorDisplayStateSynchronizer
{
    /// <summary>
    /// Recompute shell display state from the currently registered displays.
    /// </summary>
    public void Refresh();
}
}
