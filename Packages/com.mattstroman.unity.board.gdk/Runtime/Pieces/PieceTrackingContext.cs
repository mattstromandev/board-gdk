using Board.Input;

using BoardGDK.BoardAdapters;

using UnityEngine;

namespace BoardGDK.Pieces
{
/// <summary>
/// Information for tracking a <see cref="BoardContact"/> during its lifecycle.
/// </summary>
public class PieceTrackingContext : IPieceSettlingContext
{
    /// <summary>
    /// The current state of the <see cref="BoardContact"/> as observed during the current or last active frame.
    /// </summary>
    public IBoardContact Contact { get; internal set; }
    
    /// <summary>
    /// The unique key that identifies the tracked piece across frames, consisting of the contact ID and glyph ID.
    /// </summary>
    public (int contactID, int glyphID) TrackingKey => (BoardContactID, GlyphID);
    
    /// <summary>
    /// The ID of the active <see cref="BoardContact"/> this piece is linked to.
    /// </summary>
    public int BoardContactID => Contact.ContactId;

    /// <summary>
    /// The ID of the <see cref="BoardContactType.Glyph"/> the physical piece has.
    /// </summary>
    public int GlyphID => Contact.GlyphId;
    
    /// <summary>
    /// The number of frames the <see cref="BoardContact"/> has been active.
    /// </summary>
    public int NumFramesActive { get; internal set; }
        
    /// <summary>
    /// The initial screen position of the <see cref="BoardContact"/> when it was first observed.
    /// </summary>
    public Vector2 InitialScreenPosition { get; internal set; }

    /// <summary>
    /// Flag indicating if the <see cref="BoardContact"/> has been considered settled by the configured
    /// <see cref="IPieceSettlingStrategy"/>s.
    /// </summary>
    internal bool HasSettled { get; set; }
    
    /// <summary>
    /// The <see cref="IVirtualPiece"/> representing the physical piece in the digital world.
    /// </summary>
    /// <remarks>
    /// A virtual piece will only be created once the <see cref="BoardContact"/> has been settled following any configured
    /// <see cref="IPieceSettlingStrategy"/>s.
    /// </remarks>
    public IVirtualPiece VirtualPiece { get; internal set; }
}
}
