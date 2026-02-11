using BoardGDK.BoardAdapters;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Context information provided to <see cref="PieceBehavior"/>s when they are processed.
/// </summary>
public class PieceBehaviorContext : IPieceSettlingContext
{
    /// <summary>
    /// The <see cref="PieceTrackingContext"/> being actively processed.
    /// </summary>
    public PieceTrackingContext TrackingContext { get; internal set; }
    
    /// <inheritdoc cref="PieceTrackingContext.TrackingKey"/>
    public (int contactID, int glyphID) TrackingKey => TrackingContext.TrackingKey;

    /// <inheritdoc cref="PieceTrackingContext.Contact"/>
    public IBoardContact Contact => TrackingContext.Contact;
    
    /// <inheritdoc cref="PieceTrackingContext.BoardContactID"/>
    public int BoardContactID => TrackingContext.BoardContactID;

    /// <inheritdoc cref="PieceTrackingContext.GlyphID"/>
    public int GlyphID => TrackingContext.GlyphID;
    
    /// <summary>
    /// The number of frames the physical piece has been placed (i.e. considered settled).
    /// </summary>
    public int NumFramesPlaced { get; internal set; }
        
    /// <summary>
    /// The initial screen position of the physical piece when it was placed (i.e. considered settled).
    /// </summary>
    public Vector2 InitialPlacementScreenPosition { get; internal set; }
    
    /// <summary>
    /// The number of frames since the <see cref="IPieceBehavior"/> was activated. This will include any number of frames
    /// it takes to settle the activation; that is, the count starts when the behavior should start settling, not when it
    /// is actually considered settled and starts processing. This allows behaviors to have consistent timing regardless
    /// of how long it takes for the behavior to settle, and is consistent with the <see cref="PieceSystem"/>'s <see cref="PieceTrackingContext.NumFramesActive"/>.
    /// </summary>
    public int NumFramesActive { get; internal set; }
        
    /// <summary>
    /// The initial screen position of the physical piece when the <see cref="IPieceBehavior"/> was activated, before
    /// any settling.
    /// </summary>
    public Vector2 InitialScreenPosition { get; internal set; }

    /// <inheritdoc cref="PieceTrackingContext.VirtualPiece"/>
    public IVirtualPiece VirtualPiece => TrackingContext.VirtualPiece;

    /// <summary>
    /// Flag indicating if the <see cref="IPieceBehavior"/>'s activation has been considered settled by its configured
    /// <see cref="IPieceSettlingStrategy"/>s.
    /// </summary>
    internal bool HasActivationSettled { get; set; }

    /// <summary>
    /// Flag indicating if the <see cref="IPieceBehavior"/> is currently active.
    /// </summary>
    internal bool IsActive { get; set; }
    
    /// <summary>
    /// The associated <see cref="IPieceBehaviorDefinition"/>.
    /// </summary>
    public IPieceBehaviorDefinition BehaviorDefinition { get; internal set; }
    
    /// <summary>
    /// The current state of the piece behavior.
    /// </summary>
    public IPieceBehaviorState State { get; internal set; }
}
}
