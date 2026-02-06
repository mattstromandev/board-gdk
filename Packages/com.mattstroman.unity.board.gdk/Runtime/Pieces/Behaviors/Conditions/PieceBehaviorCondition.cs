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
    /// <remarks>
    /// Override only if you need to add additional validation checks <b>before</b> invoking the actual evaluation in
    /// <see cref="DoEvaluation"/>. For example, you may have additional inputs that are required for evaluation.
    /// </remarks>
    /// <example>
    /// <code>
    /// public override bool Evaluate(PieceBehaviorContext context)
    /// {
    ///     if(MyCustomSetting == null)
    ///     {
    ///         UnityEngine.Debug.LogWarning($"{GetType().Name}: {nameof(MyCustomSetting)} is null; unable to evaluate condition. Context: {JsonUtility.ToJson(context)}"); 
    ///         return false;
    ///     }
    /// 
    ///     return base.Evaluate(context);
    /// }
    /// </code>
    /// </example>
    public virtual bool Evaluate(PieceBehaviorContext context)
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
