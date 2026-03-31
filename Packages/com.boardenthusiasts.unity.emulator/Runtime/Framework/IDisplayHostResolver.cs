using JetBrains.Annotations;

namespace BE.Emulator.Framework
{
/// <summary>
/// Resolves the host element that should contain a given <see cref="IEmulatorDisplay"/>.
/// </summary>
public interface IDisplayHostResolver
{
    /// <summary>
    /// Resolve the host configuration for the provided <paramref name="display"/>.
    /// </summary>
    /// <param name="display">The display that needs a host.</param>
    /// <returns>The resolved host configuration.</returns>
    public DisplayHostResolution Resolve([NotNull] IEmulatorDisplay display);
}
}
