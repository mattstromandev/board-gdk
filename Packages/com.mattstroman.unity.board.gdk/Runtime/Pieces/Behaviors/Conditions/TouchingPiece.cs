using System;

namespace BoardGDK.Pieces.Behaviors.Conditions
{
/// <summary>
/// Condition that stipulates the piece is currently being touched.
/// </summary>
[Serializable]
public class TouchingPiece : PieceBehaviorCondition
{
    /// <inheritdoc />
    protected override bool DoEvaluation(PieceBehaviorContext context)
    {
        return context.ActiveContact.isTouched;
    }
}
}
