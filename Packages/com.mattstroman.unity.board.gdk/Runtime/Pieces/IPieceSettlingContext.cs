using Board.Input;

using BoardGDK.BoardAdapters;

using UnityEngine;

namespace BoardGDK.Pieces
{
/// <summary>
/// Interface providing context information about a piece being settled.
/// </summary>
public interface IPieceSettlingContext
{
    /// <summary>
    /// The current state of the <see cref="BoardContact"/> as observed during the current or last active frame.
    /// </summary>
    public IBoardContact Contact { get; }

    /// <summary>
    /// The unique key that identifies the pieces across frames, consisting of the contact ID and glyph ID.
    /// </summary>
    public (int contactID, int glyphID) TrackingKey => (BoardContactID, GlyphID);
    
    /// <summary>
    /// The ID of the active <see cref="BoardContact"/> the piece is linked to.
    /// </summary>
    public int BoardContactID => Contact.ContactId;

    /// <summary>
    /// The ID of the <see cref="BoardContactType.Glyph"/> the physical piece has.
    /// </summary>
    public int GlyphID => Contact.GlyphId;
    
    /// <summary>
    /// The number of frames the <see cref="BoardContact"/> has been active.
    /// </summary>
    public int NumFramesActive { get; }
        
    /// <summary>
    /// The initial screen position of the <see cref="BoardContact"/> when it was first observed.
    /// </summary>
    public Vector2 InitialScreenPosition { get; }
}
}
