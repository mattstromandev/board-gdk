using System;

using Rahmen.Logging;

namespace BE.Emulator
{
/// <summary>
/// Log channel declarations used by the BE Emulator for Board package.
/// </summary>
public static class LogChannels
{
    /// <summary>
    /// <see cref="LogChannel"/> for the <see cref="BE.Emulator"/> namespace.
    /// </summary>
    [Serializable]
    public sealed class BoardEmulation : LogChannel
    {
        /// <inheritdoc />
        protected override string InitialDescription => "Logs relating to emulation of the Board console in the Unity editor.";
    }
}
}
