using System;

using Rahmen.Events;

using UnityEngine;

namespace BoardGDK.Pieces.Events
{
/// <summary>
/// Base <see cref="IRahmenEvent"/> wrapping a <see cref="PieceEvent"/>. Used to create specific events involving pieces
/// that can be raised through the <see cref="IEventSystem"/>.
/// </summary>
[Serializable]
public abstract class PieceRahmenEvent : PieceEvent, IRahmenEvent
{
    [SerializeField]
    [Tooltip("The virtual piece involved in this event.")]
    private VirtualPiece m_virtualPiece;

    public override IVirtualPiece VirtualPiece
    {
        get => m_virtualPiece;
        set => m_virtualPiece = value as VirtualPiece;
    }
}
}
