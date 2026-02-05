using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace BoardGDK.Pieces.Behaviors.Conditions
{
/// <summary>
/// Base class for an <see cref="IPieceBehaviorCondition"/> which depends on another piece's behavior.
/// </summary>
[Serializable]
public abstract class DependentPieceCondition : PieceBehaviorCondition
{
    /// <summary>
    /// The <see cref="PieceBehaviorDefinition"/> on which this condition depends.
    /// </summary>
    [field: Tooltip("The definition for piece behavior on which this condition depends.")]
    [field: SerializeField]
    protected PieceBehaviorDefinition DependentPieceBehavior { get; private set; }
}
}
