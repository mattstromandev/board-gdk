using Board.Input;

using BoardGDK.Pieces.Behaviors.Conditions;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Context information provided to <see cref="PieceBehavior"/>s when they are processed.
/// </summary>
public struct PieceBehaviorContext
{
    /// <summary>
    /// The <see cref="BoardContact"/> being actively processed.
    /// </summary>
    public BoardContact ActiveContact { get; set; }

    /// <summary>
    /// The <see cref="VirtualPiece"/> to apply behavior to.
    /// </summary>
    public IVirtualPiece VirtualPiece { get; set; }
    
    /// <summary>
    /// The <see cref="IPieceBehaviorDefinition"/>
    /// </summary>
    public IPieceBehaviorDefinition BehaviorDefinition { get; set; }

    /// <summary>
    /// Flag indicating whether the behavior meets the global <see cref="IPieceBehaviorCondition"/>s.
    /// </summary>
    public bool MeetsGlobalConditions { get; set; }
}
}
