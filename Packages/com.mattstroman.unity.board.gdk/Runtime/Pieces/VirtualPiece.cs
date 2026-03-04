using System;
using System.Collections.Generic;

using Board.Input;

using BoardGDK.BoardAdapters;
using BoardGDK.Pieces.Behaviors;
using BoardGDK.Pieces.Events;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces
{
/// <summary>
/// In-game virtual state of a physical piece.
/// </summary>
public class VirtualPiece : MonoBehaviour, IVirtualPiece
{
    // BoardContact is serialized in editor only for ease of debugging, but results in a boxing per frame, so it is compiled
    // out for runtime. However, we may even want to add a scripting define to enable/disable it in the editor as well,
    // if we find that the boxing is causing performance issues in the editor. It could be an opt-in-only feature for
    // debugging purposes and piece behavior composition.
#if UNITY_EDITOR
    [SerializeField]
    [Tooltip("The Board contact that this virtual piece is synced to")]
    // ReSharper disable once NotAccessedField.Local
    // Justification: Serialized for debugging purposes in editor only; not meant to be accessed directly.
    private SerializableBoardContact m_boardContact;
#endif

    [SerializeField]
    [Tooltip("The GameObjects currently representing this piece in the digital world, if there are any digital representation.")]
    private List<GameObject> m_digitalPieces = new();

    private BoardContact _nonSerializedBoardContact;
    private PieceBehaviorSettings _pieceBehaviorSettings;
    private IPieceSystem _pieceSystem;
    private readonly Dictionary<EventHandler<IPieceEvent>, EventHandler<IPieceEvent>> _pickedUpEventWrappers = new();

    /// <inheritdoc/>
    public BoardContact BoardContact
    {
        get => _nonSerializedBoardContact;
        internal set 
        {
        #if UNITY_EDITOR
            m_boardContact = new SerializableBoardContact(value);
        #endif
            
            _nonSerializedBoardContact = value;
        }
    }

    /// <inheritdoc/>
    public Transform AnchorTransform => transform;

    /// <inheritdoc/>
    public IReadOnlyCollection<GameObject> DigitalPieces => m_digitalPieces;

    /// <inheritdoc/>
    public event EventHandler<IPieceEvent> PickedUp
    {
        add
        {
            if(_pickedUpEventWrappers.TryGetValue(value, out EventHandler<IPieceEvent> wrapper)) { return; }

            wrapper = (sender, args) =>
            {
                if(ReferenceEquals(args.VirtualPiece, this) == false) { return; }
                value(sender, args);
            };
            _pickedUpEventWrappers.Add(value, wrapper);
            _pieceSystem.PiecePickedUp += wrapper;
        }
        remove
        {
            if(_pickedUpEventWrappers.Remove(value, out EventHandler<IPieceEvent> wrapper))
            {
                _pieceSystem.PiecePickedUp -= wrapper;
            }
        }
    }

    /// <inheritdoc/>
    void IVirtualPiece.AddDigitalPiece(GameObject digitalPiece)
    {
        m_digitalPieces.Add(digitalPiece);
    }

    /// <inheritdoc/>
    public void RemoveDigitalPiece(GameObject digitalPiece)
    {
        m_digitalPieces.Remove(digitalPiece);
    }

    [Inject]
    private void Injection(IPieceBehaviorSettings pieceBehaviorSettings, IPieceSystem pieceSystem)
    {
        if(pieceBehaviorSettings is PieceBehaviorSettings castPieceBehaviorSettings)
        {
            _pieceBehaviorSettings = castPieceBehaviorSettings;
        }
        _pieceSystem = pieceSystem;
    }
}
}
