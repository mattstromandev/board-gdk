using System.Collections.Generic;

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

    [SerializeField]
    [Tooltip("The GameObjects currently representing this piece in the digital world, if there are any digital representation.")]
    // TODO: Make this readonly in the inspector
    private List<GameObject> m_digitalPieces = new();

    /// <inheritdoc/>
    public Transform AnchorTransform => transform;

    /// <inheritdoc/>
    public IReadOnlyCollection<GameObject> DigitalPieces => m_digitalPieces;

    /// <inheritdoc/>
    void IVirtualPiece.AddDigitalPiece(GameObject digitalPiece)
    {
        m_digitalPieces.Add(digitalPiece);
    }

    public void RemoveDigitalPiece(GameObject digitalPiece)
    {
        m_digitalPieces.Remove(digitalPiece);
    }
}
}
