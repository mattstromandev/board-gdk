using System;

using Board.Input;

using BoardGDK.Extensions.Board;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> which syncs the position of a digital piece to that of the physical Piece on Board.
/// </summary>
[Serializable]
public class SyncPosition : PieceBehavior
{
    [Tooltip("The axis treated as world up when determining the movement plane.")]
    [SerializeField]
    private Vector3 m_worldUpAxis = Vector3.up;

    [Tooltip("Layers to raycast against when resolving the surface position.")]
    [SerializeField]
    private LayerMask m_surfaceLayers = ~0;

    /// <inheritdoc />
    protected override void OnActivate(PieceBehaviorContext context) { SetPosition(context); }

    /// <inheritdoc />
    protected override void OnUpdate(PieceBehaviorContext context)
    {
        if(context.ActiveContact.phase != BoardContactPhase.Moved) { return; }

        SetPosition(context);
    }

    protected override void OnDeactivate(PieceBehaviorContext context) { }

    private void SetPosition(PieceBehaviorContext context)
    {
        if(context.VirtualPiece.DigitalPiece == null) { return; }

        Camera mainCamera = Camera.main;
        if(mainCamera == null) { return; }

        Vector3 currentWorldPosition = context.VirtualPiece.DigitalPiece.transform.position;
        Vector3 targetWorldPosition = context.ActiveContact.GetWorldPosition(m_worldUpAxis, m_surfaceLayers, currentWorldPosition);
        
        context.VirtualPiece.DigitalPiece.transform.position = targetWorldPosition;
    }
}
}
