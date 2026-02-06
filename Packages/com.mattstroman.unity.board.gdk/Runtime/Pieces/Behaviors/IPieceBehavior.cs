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
    /// Process an active <see cref="BoardContact"/> for a <see cref="VirtualPiece"/> according to this behavior.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    public void ProcessContact(PieceBehaviorContext context);
}
}
