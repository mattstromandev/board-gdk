namespace BE.Emulator.Framework
{
/// <summary>
/// Interface for a display that can appear in the <see cref="EmulatorView"/>.
/// </summary>
public interface IEmulatorDisplay : IDisplayable
{
    /// <summary>
    /// The name of this display.
    /// </summary>
    public string Name { get; }
}
}
