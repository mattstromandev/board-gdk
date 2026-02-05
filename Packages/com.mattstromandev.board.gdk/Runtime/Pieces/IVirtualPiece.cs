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
    /// The ID of the active <see cref="BoardContact"/> this piece is linked to.
    /// </summary>
    public int BoardContactID { get; }

    /// <summary>
    /// The ID of the <see cref="BoardContactType.Glyph"/> the physical piece has.
    /// </summary>
    public int GlyphID { get; }

    /// <summary>
    /// The <see cref="GameObject"/> currently representing this piece in the digital world, if there is a digital representation.
    /// </summary>
    public GameObject DigitalPiece { get; }
    
    /// <summary>
    /// Set the <see cref="DigitalPiece"/>.
    /// </summary>
    /// <param name="digitalPiece">The <see cref="GameObject"/> to use for the digital piece.</param>
    internal void AssignDigitalPiece(GameObject digitalPiece);
}
}
