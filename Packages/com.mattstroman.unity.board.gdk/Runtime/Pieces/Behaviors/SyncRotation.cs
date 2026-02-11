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
    
    [Tooltip("An offset (in degrees and screen space) to apply to the rotation.")]
    [SerializeField]
    private float m_offsetDegrees;
    
    [Tooltip("If greater than zero, the rotation will snap to increments of this many degrees.")]
    [SerializeField]
    private int m_snapStep;

    /// <inheritdoc />
    public override void Place(PieceBehaviorContext context) { }

    /// <inheritdoc />
    public override void Activate(PieceBehaviorContext context) { SetRotation(context); }

    /// <inheritdoc />
    public override void Update(PieceBehaviorContext context)
    {
        // TODO: Handle the ability to specify if this should be updated in more than just move.
        if(context.Contact.phase != BoardContactPhase.Moved) { return; }

        SetRotation(context);
    }

    /// <inheritdoc />
    public override void Deactivate(PieceBehaviorContext context) { }

    /// <inheritdoc />
    public override void PickUp(PieceBehaviorContext context) { }

    private void SetRotation(PieceBehaviorContext context)
    {
        float baseRotationDegrees = context.Contact.orientation * Mathf.Rad2Deg + m_offsetDegrees;

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
