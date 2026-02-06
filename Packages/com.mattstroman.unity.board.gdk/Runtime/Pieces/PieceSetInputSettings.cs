using System.Collections.Generic;

using AYellowpaper.SerializedCollections;

using Board.Input;

using UnityEngine;

namespace BoardGDK.Pieces
{
/// <summary>
/// Extension of <see cref="BoardInputSettings"/> to help define user-friendly settings for the Board SDK.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece Set Input Settings")]
public class PieceSetInputSettings : BoardInputSettings
{
    [SerializeField]
    [SerializedDictionary("Glyph ID", "Piece Name")]
    [Tooltip("The mapping of glyph IDs to Piece names for this piece set.")]
    private SerializedDictionary<int, string> m_glyphIDMapping = new();
    
    /// <summary>
    /// The mapping of glyph IDs to Piece names for this piece set.
    /// </summary>
    public IReadOnlyDictionary<int, string> GlyphIDMapping => m_glyphIDMapping;
    
    /// <summary>
    /// The name of the piece set model.
    /// </summary>
    public string PieceSetModelName => name;
}
}
