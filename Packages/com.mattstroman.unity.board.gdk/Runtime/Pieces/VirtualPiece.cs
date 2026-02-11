using System;
using System.Collections.Generic;

using BoardGDK.BoardAdapters;

using UnityEngine;

namespace BoardGDK.Pieces
{
/// <summary>
/// In-game virtual state of a physical piece.
/// </summary>
public class VirtualPiece : MonoBehaviour, IVirtualPiece
{
    [SerializeField]
    [Tooltip("The Board contact that this virtual piece is synced to")]
    private SerializableBoardContact m_boardContact;

    [SerializeField]
    [Tooltip("The GameObjects currently representing this piece in the digital world, if there are any digital representation.")]
    private List<GameObject> m_digitalPieces = new();

    /// <inheritdoc/>
    public IBoardContact BoardContact
    {
        get => m_boardContact;
        internal set
        {
            if(value.GetType() != typeof(SerializableBoardContact))
            {
                throw new ArgumentException($"Type of value must be {nameof(SerializableBoardContact)}.", nameof(value));
            }
            m_boardContact = (SerializableBoardContact)value;
        }
    }

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
