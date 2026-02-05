using Board.Input;

using UnityEngine;

namespace BoardGDK.Pieces
{
/// <summary>
/// In-game virtual state of a physical piece.
/// </summary>
public class VirtualPiece : MonoBehaviour, IVirtualPiece
{
    /// <inheritdoc/>
    [field: SerializeField]
    [field: Tooltip("The ID of the active BoardContact this piece is linked to.")]
    // TODO: Make this readonly in the inspector
    public int BoardContactID { get; internal set; }

    /// <inheritdoc/>
    [field: SerializeField]
    [field: Tooltip("The glyph ID of the associated physical Piece.")]
    // TODO: Make this readonly in the inspector
    public int GlyphID { get; internal set; }

    /// <inheritdoc/>
    [field: SerializeField]
    [field: Tooltip("The GameObject currently representing this piece in the digital world, if there is a digital representation.")]
    // TODO: Make this readonly in the inspector
    public GameObject DigitalPiece { get; internal set; }

    /// <inheritdoc/>
    void IVirtualPiece.AssignDigitalPiece(GameObject digitalPiece)
    {
        DigitalPiece = digitalPiece;
    }
}
}
