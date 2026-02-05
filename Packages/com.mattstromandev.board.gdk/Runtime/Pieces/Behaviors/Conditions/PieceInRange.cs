using System;

using BoardGDK.Extensions;
using BoardGDK.Extensions.Board;
using BoardGDK.Extensions.Pieces;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors.Conditions
{
/// <summary>
/// <see cref="DependentPieceCondition"/> that stipulates a matching Piece is currently in range of another. 
/// </summary>
[Serializable]
public class PieceInRange : DependentPieceCondition
{
    [SerializeField] private float m_rangeRadius;

    private IPieceSystem _pieceSystem;

    /// <inheritdoc />
    protected override bool DoEvaluation(PieceBehaviorContext context)
    {
        Vector3 activeContactWorldPosition = context.ActiveContact.GetWorldPosition();
        
        return _pieceSystem?.IsPieceInRange(DependentPieceBehavior, activeContactWorldPosition, m_rangeRadius) ?? false;
    }

    [Inject]
    private void Injection(IPieceSystem pieceSystem) { _pieceSystem = pieceSystem; }
}
}
