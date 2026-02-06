using System;

using Zenject;

namespace BoardGDK.Pieces.Behaviors.Conditions
{
/// <summary>
/// Condition that stipulates a matching Piece is currently active on Board. 
/// </summary>
[Serializable]
public class PieceOnBoard : DependentPieceCondition
{
    private IPieceSystem _pieceSystem;

    /// <inheritdoc />
    protected override bool DoEvaluation(PieceBehaviorContext context)
    {
        return _pieceSystem?.IsOnBoard(DependentPieceBehavior) ?? false;
    }

    [Inject]
    private void Injection(IPieceSystem pieceSystem) { _pieceSystem = pieceSystem; }
}
}
