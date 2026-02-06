using System;

using Zenject;

namespace BoardGDK.Pieces.Behaviors.Conditions
{
/// <summary>
/// Condition stipulating that a matching Piece must currently be touched. 
/// </summary>
[Serializable]
public class PieceIsTouched : DependentPieceCondition
{
    private IPieceSystem _pieceSystem;

    /// <inheritdoc />
    protected override bool DoEvaluation(PieceBehaviorContext context)
    {
        return _pieceSystem?.IsTouched(DependentPieceBehavior) ?? false;
    }

    [Inject]
    private void Injection(IPieceSystem pieceSystem) { _pieceSystem = pieceSystem; }
}
}
