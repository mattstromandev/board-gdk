using System;

using BoardGDK.Data;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> that stores the current and delta orientations of the Piece to <see cref="FloatVariable"/>s.
/// </summary>
[Serializable]
public class StoreOrientation : PieceBehavior
{
    [Tooltip("Variable to store the current orientation of the piece in degrees.")]
    [SerializeField]
    private FloatVariable m_pieceOrientation;
    
    [Tooltip("Variable to store the change in orientation of the piece in degrees since the last update.")]
    [SerializeField]
    private FloatVariable m_pieceOrientationDelta;

    private float _lastOrientationDegrees;

    /// <inheritdoc />
    public override void Place(PieceBehaviorContext context) { }

    /// <inheritdoc />
    public override void Activate(PieceBehaviorContext context)
    {
        _lastOrientationDegrees = context.ContactState.orientation;
    }

    /// <inheritdoc />
    public override void Update(PieceBehaviorContext context)
    {
        float orientationDegrees = context.ContactState.orientation * Mathf.Rad2Deg;

        if(m_pieceOrientation != null) { m_pieceOrientation.Value = orientationDegrees; }

        if(m_pieceOrientationDelta != null)
        {
            float rawDeltaDegrees = orientationDegrees - _lastOrientationDegrees;

            // Account for the 0/360 line so that it is only ever a degree delta < 360
            float orientationDeltaDegrees = (360 + rawDeltaDegrees) % 360;

            m_pieceOrientationDelta.Value = orientationDeltaDegrees;
            _lastOrientationDegrees = orientationDegrees;
        }
    }

    /// <inheritdoc />
    public override void Deactivate(PieceBehaviorContext context) { }

    /// <inheritdoc />
    public override void PickUp(PieceBehaviorContext context) { }
}
}
