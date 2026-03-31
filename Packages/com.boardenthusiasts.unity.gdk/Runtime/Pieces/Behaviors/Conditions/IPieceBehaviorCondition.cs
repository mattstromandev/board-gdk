namespace BoardGDK.Pieces.Behaviors.Conditions
{
/// <summary>
/// Interface for conditions that determine when a <see cref="IPieceBehavior"/> is applied.
/// </summary>
public interface IPieceBehaviorCondition
{
    /// <summary>
    /// Flag indicating whether to negate the result of this condition.
    /// </summary>
    public bool Negate { get; }
    
    /// <summary>
    /// Evaluate this condition against the provided context.
    /// </summary>
    /// <param name="context">The necessary context for evaluating the condition.</param>
    /// <returns>True if the condition is met; false otherwise.</returns>
    public bool Evaluate(PieceBehaviorContext context);
}
}
