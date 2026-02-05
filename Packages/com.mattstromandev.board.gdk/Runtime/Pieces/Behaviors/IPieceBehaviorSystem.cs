using Board.Input;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Interface for systems that manage piece behaviors.
/// </summary>
public interface IPieceBehaviorSystem
{
    /// <summary>
    /// The <see cref="IPieceBehaviorSettings"/> to apply globally to all <see cref="IPieceBehavior"/>s.
    /// </summary>
    public IPieceBehaviorSettings GlobalSettings { get; }
    
    /// <summary>
    /// The <see cref="IPieceBehaviorPrioritySettings"/> determining execution priorities for <see cref="IPieceBehavior"/>s.
    /// </summary>
    public IPieceBehaviorPrioritySettings PrioritySettings { get; }
    
    /// <summary>
    /// Process behaviors for an active contact on Board.
    /// </summary>
    /// <param name="boardContact">The active <see cref="BoardContact"/> to process.</param>
    /// <param name="virtualPiece">The <see cref="IVirtualPiece"/> associated with the active contact.</param>
    public void ProcessBoardContact(BoardContact boardContact, IVirtualPiece virtualPiece);
}
}
