using System;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Base class for a <see cref="PieceBehavior"/> which depends on another piece.
/// </summary>
[Serializable]
public abstract class DependentPieceBehavior : PieceBehavior
{
    /// <summary>
    /// The <see cref="PieceBehaviorDefinition"/> for the piece on which this behavior depends.
    /// </summary>
    [field: Tooltip("The definition for piece behavior on which this behavior depends.")]
    [field: SerializeField]
    protected PieceBehaviorDefinition DependentPiece { get; private set; }
}
}
