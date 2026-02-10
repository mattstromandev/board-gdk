using Board.Input;

using BoardGDK.Pieces.Behaviors.Conditions;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Interface for behaviors applied to <see cref="VirtualPiece"/>s.
/// </summary>
public interface IPieceBehavior
{
    /// <summary>
    /// Optional execution order for this behavior. Lower numbers execute first.
    /// </summary>
    public int ExecutionOrder { get; }
    
    /// <summary>
    /// Set of <see cref="IPieceBehaviorCondition"/>s determining when this behavior is applied.
    /// </summary>
    public IPieceBehaviorCondition[] Conditions { get; }

    /// <summary>
    /// Flag indicating whether to override the global <see cref="IPieceBehaviorCondition"/>s for this behavior. If
    /// true, the global conditions are ignored and this behavior uses only its own <see cref="Conditions"/>.
    /// </summary>
    public bool OverrideGlobalConditions { get; }

    /// <summary>
    /// Flag indicating whether the global <see cref="IPieceBehaviorSettings"/> are overridden for this behavior. If
    /// true, see the <see cref="OverrideSettings"/> for the settings to use.
    /// </summary>
    public bool GlobalSettingsOverridden { get; }

    /// <summary>
    /// The <see cref="IPieceBehaviorSettings"/> to use for this behavior.
    /// </summary>
    public IPieceBehaviorSettings OverrideSettings { get; }

    /// <summary>
    /// Determine if this behavior's activation is considered settled according to its <see cref="IPieceSettlingStrategy"/>s.
    /// </summary>
    /// <param name="context">The <see cref="IPieceSettlingContext"/> providing necessary information for settling logic.</param>
    /// <returns>True if the behavior activation has settled; false otherwise.</returns>
    public bool HasSettled(IPieceSettlingContext context);

    /// <summary>
    /// Evaluate the conditions for this behavior against the given context to determine if this behavior should be applied.
    /// </summary>
    /// <param name="context">The <see cref="IPieceSettlingContext"/> providing necessary information for evaluation.</param>
    /// <returns>True if the conditions are met; false otherwise.</returns>
    public bool EvaluateConditions(PieceBehaviorContext context);

    /// <summary>
    /// Handle placing a new active <see cref="BoardContact"/> according to this behavior.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    public void Place(PieceBehaviorContext context);

    /// <summary>
    /// Activate this behavior.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    public void Activate(PieceBehaviorContext context);

    /// <summary>
    /// Update this behavior.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    public void Update(PieceBehaviorContext context);

    /// <summary>
    /// Deactivate this behavior.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    public void Deactivate(PieceBehaviorContext context);

    /// <summary>
    /// Handle removing an exiting <see cref="BoardContact"/> according to this behavior.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    public void PickUp(PieceBehaviorContext context);
}
}
