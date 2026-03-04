namespace BoardGDK.Pieces.Events
{
/// <summary>
/// Interface for events involving an <see cref="IVirtualPiece"/> (e.g. being picked up, placed, etc.).
/// </summary>
public interface IPieceEvent
{
    /// <summary>
    /// The <see cref="IVirtualPiece"/> involved.
    /// </summary>
    public IVirtualPiece VirtualPiece { get; set; }
}
}
