using System;

namespace BoardGDK.Pieces.Events
{
/// <summary>
/// Event involving an <see cref="IVirtualPiece"/> (e.g. being picked up, placed, etc.).
/// </summary>
public class PieceEvent : EventArgs, IPieceEvent
{
    public virtual IVirtualPiece VirtualPiece { get; set; }
}
}
