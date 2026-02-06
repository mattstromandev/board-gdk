using System;

using Board.Input;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> which syncs the rotation of a digital piece to that of the physical Piece on Board.
/// </summary>
[Serializable]
public class SyncRotation : PieceBehavior
{
    [Tooltip("The axis treated as world up when applying rotation (world space).")]
    [SerializeField]
    private Vector3 m_worldUpAxis = Vector3.up;
    
    [Tooltip("An offset (in degrees), if any, to apply to the rotation.")]
    [SerializeField]
    private float m_offsetDegrees;
    
    [Tooltip("If greater than zero, the rotation will snap to increments of this many degrees.")]
    [SerializeField]
    private int m_snapStep;

    /// <inheritdoc />
    protected override void OnActivate(PieceBehaviorContext context) { SetRotation(context); }

    /// <inheritdoc />
    protected override void OnUpdate(PieceBehaviorContext context)
    {
        if(context.ActiveContact.phase != BoardContactPhase.Moved) { return; }

        SetRotation(context);
    }

    protected override void OnDeactivate(PieceBehaviorContext context) { }

    private void SetRotation(PieceBehaviorContext context)
    {
        float baseRotationDegrees = context.ActiveContact.orientation * Mathf.Rad2Deg + m_offsetDegrees;

        if(m_snapStep > 0) { baseRotationDegrees = Mathf.RoundToInt(baseRotationDegrees / m_snapStep) * m_snapStep; }

        Vector3 rotationAxis = ResolveWorldUpAxis();
        context.VirtualPiece.AnchorTransform.rotation = Quaternion.AngleAxis(baseRotationDegrees, rotationAxis);
    }

    private Vector3 ResolveWorldUpAxis()
    {
        if(m_worldUpAxis.sqrMagnitude <= Mathf.Epsilon) { return Vector3.forward; }

        return m_worldUpAxis.normalized;
    }
}
}
