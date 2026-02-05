using System;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors.Conditions
{
/// <summary>
/// Base <see cref="IPieceBehaviorCondition"/> implementation that adds general support for negation.
/// </summary>
[Serializable]
public abstract class PieceBehaviorCondition : IPieceBehaviorCondition
{
    /// <inheritdoc />
    [field: SerializeField]
    [field: Tooltip("Flag indicating whether to negate the result of this condition.")]
    public bool Negate { get; private set; }

    /// <inheritdoc />
    public bool Evaluate(PieceBehaviorContext context)
    {
        bool result = DoEvaluation(context);
        
        return Negate ? !result : result;
    }
    
    /// <summary>
    /// Implement to evaluate this condition against the provided context.
    /// </summary>
    /// <param name="context">The necessary context for evaluating the condition.</param>
    /// <returns>True if the condition is met; false otherwise.</returns>
    protected abstract bool DoEvaluation(PieceBehaviorContext context);
}
}
