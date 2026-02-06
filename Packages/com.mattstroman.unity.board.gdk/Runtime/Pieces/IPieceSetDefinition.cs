using System.Collections.Generic;

using BoardGDK.Pieces.Behaviors;

namespace BoardGDK.Pieces
{
/// <summary>
/// Interface defining a Board Piece set.
/// </summary>
public interface IPieceSetDefinition
{
    /// <summary>
    /// The name of the piece set model.
    /// </summary>
    public string PieceSetName { get; }

    /// <summary>
    /// The <see cref="PieceSetInputSettings"/> associated with this piece set.
    /// </summary>
    public PieceSetInputSettings InputSettings { get; }

    /// <summary>
    /// The mapping of glyph IDs to Piece names for this piece set.
    /// </summary>
    public IReadOnlyDictionary<int, string> GlyphIDMapping { get; }

    /// <summary>
    /// The definitions of <see cref="IPieceBehavior"/>s associated with this piece set.
    /// </summary>
    public IReadOnlyCollection<IPieceBehaviorDefinition> PieceBehaviorDefinitions { get; }
}
}
