using System.Collections.Generic;

using Board.Input;

using UnityEngine;

namespace BoardGDK.Pieces
{
/// <summary>
/// Interface for an in-game virtual state of a physical piece.
/// </summary>
public interface IVirtualPiece
{
    /// <summary>
    /// The name of the <see cref="IVirtualPiece"/>.
    /// </summary>
    public string Name => AnchorTransform.name;

    /// <summary>
    /// The ID of the active <see cref="BoardContact"/> this piece is linked to.
    /// </summary>
    public int BoardContactID => BoardContact.contactId;

    /// <summary>
    /// The ID of the <see cref="BoardContactType.Glyph"/> the physical piece has.
    /// </summary>
    public int GlyphID => BoardContact.glyphId;

    /// <summary>
    /// The <see cref="BoardContact"/> this <see cref="IVirtualPiece"/> is synced to.
    /// </summary>
    public BoardContact BoardContact { get; }
    
    /// <summary>
    /// The <see cref="Transform"/> that acts as the anchor for this piece in the digital world.
    /// </summary>
    public Transform AnchorTransform { get; }

    /// <summary>
    /// The <see cref="GameObject"/>s currently representing this piece in the digital world, if there are any digital representation.
    /// </summary>
    /// <remarks>
    /// These object will all be direct children of the <see cref="AnchorTransform"/>.
    /// </remarks>
    public IReadOnlyCollection<GameObject> DigitalPieces { get; }
    
    /// <summary>
    /// Add a <see cref="DigitalPieces"/>.
    /// </summary>
    /// <param name="digitalPiece">The <see cref="GameObject"/> to add as a digital piece.</param>
    public void AddDigitalPiece(GameObject digitalPiece);
    
    /// <summary>
    /// Remove a <see cref="DigitalPieces"/>, if it is added.
    /// </summary>
    /// <param name="digitalPiece">The <see cref="GameObject"/> to remove from the digital pieces.</param>
    public void RemoveDigitalPiece(GameObject digitalPiece);
}
}
