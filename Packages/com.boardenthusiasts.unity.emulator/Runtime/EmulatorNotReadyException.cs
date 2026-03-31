using System;

namespace BE.Emulator
{
/// <summary>
/// Thrown when the emulator's static Board API surface is used before the emulator has finished initializing.
/// </summary>
public sealed class EmulatorNotReadyException : InvalidOperationException
{
    public EmulatorNotReadyException()
        : base("The BE Emulator for Board static API was accessed before the emulator bindings were initialized.")
    {
    }

    public EmulatorNotReadyException(string message)
        : base(message)
    {
    }

    public EmulatorNotReadyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
}
