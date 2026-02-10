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
    /// Handle placement of a virtual piece in the digital world for an active <see cref="BoardContact"/> on Board.
    /// </summary>
    /// <param name="trackingContext">The <see cref="PieceTrackingContext"/> associated with the active <see cref="BoardContact"/>.</param>
    public void Place(PieceTrackingContext trackingContext);

    /// <summary>
    /// Process behaviors for an active <see cref="BoardContact"/> on Board.
    /// </summary>
    /// <param name="trackingContext">The <see cref="PieceTrackingContext"/> associated with the active <see cref="BoardContact"/>.</param>
    public void Update(PieceTrackingContext trackingContext);
    
    /// <summary>
    /// Handle removal of a virtual piece from the digital world for a <see cref="BoardContact"/> that is no longer active on Board.
    /// </summary>
    /// <param name="trackingContext">The <see cref="PieceTrackingContext"/> associated with the removed <see cref="BoardContact"/>.</param>
    public void PickUp(PieceTrackingContext trackingContext);
}
}
