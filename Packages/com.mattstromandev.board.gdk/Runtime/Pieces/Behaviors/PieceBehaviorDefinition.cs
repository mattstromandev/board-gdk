using System;

using BoardGDK.Pieces.Behaviors.Conditions;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Data object defining behaviors and conditions that apply when a physical Piece is on the Board.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece Behavior Definition")]
public class PieceBehaviorDefinition : ScriptableObject, IPieceBehaviorDefinition
{
    /// <inheritdoc />
    [field: Tooltip("Board Pieces with these glyph IDs will match this definition.")]
    [field: SerializeField]
    public int[] GlyphIDs { get; private set; } = Array.Empty<int>();

    /// <inheritdoc />
    [field: Tooltip("The piece behavior sets that will be processed when Pieces with glyph IDs matching those configured are on the Board.")]
    [field: SerializeField]
    public PieceBehaviorSet[] BehaviorSets { get; private set; } = Array.Empty<PieceBehaviorSet>();

    /// <inheritdoc />
    [field: Tooltip("Any behaviors that will be processed in addition to those defined in the behavior sets when Pieces with glyph IDs matching those configured are on the Board.")]
    [field: SerializeReference, SubclassSelector]
    public IPieceBehavior[] Behaviors { get; private set; } = Array.Empty<IPieceBehavior>();

    /// <inheritdoc />
    [field: Tooltip("The conditions that apply for all behaviors referenced in this definition.")]
    [field: SerializeReference, SubclassSelector]
    public IPieceBehaviorCondition[] GlobalConditions { get; private set; } = Array.Empty<IPieceBehaviorCondition>();
}
}
