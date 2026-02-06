using BoardGDK.Pieces.Behaviors.Conditions;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Interface for defining behaviors and conditions that apply when a physical Piece is on the Board.
/// </summary>
public interface IPieceBehaviorDefinition
{
    /// <summary>
    /// The glyph IDs to which this definition should apply.
    /// </summary>
    /// <remarks>
    /// Note that, while it is typical for a piece definition to apply only to a single glyph ID, it is possible that you
    /// may want to have different 
    /// </remarks>
    public int[] GlyphIDs { get; }

    /// <summary>
    /// The <see cref="PieceBehaviorSet"/>s that will be applied.
    /// </summary>
    public PieceBehaviorSet[] BehaviorSets { get; }

    /// <summary>
    /// Any <see cref="PieceBehavior"/>s that will be applied in addition to those defined in the <see cref="BehaviorSets"/>.
    /// </summary>
    public IPieceBehavior[] Behaviors { get; }

    /// <summary>
    /// The <see cref="IPieceBehaviorCondition"/>s that apply for all behaviors referenced in this definition.
    /// </summary>
    public IPieceBehaviorCondition[] GlobalConditions { get; }
}
}
