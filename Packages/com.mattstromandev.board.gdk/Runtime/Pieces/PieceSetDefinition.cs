using System;
using System.Collections.Generic;

using BoardGDK.Pieces.Behaviors;

using UnityEngine;

namespace BoardGDK.Pieces
{
/// <summary>
/// <see cref="IPieceSetDefinition"/> exposed to the Unity editor for data-driven piece sets.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece Set Definition")]
public class PieceSetDefinition : ScriptableObject, IPieceSetDefinition
{
    /// <inheritdoc />
    public string PieceSetName => name;

    /// <inheritdoc />
    [field: SerializeField]
    [field: Tooltip("The Board Input Settings associated with this Piece Set.")]
    public PieceSetInputSettings InputSettings { get; private set; }

    [SerializeField]
    [Tooltip("The definitions of Piece Behaviors associated with this Piece Set.")]
    private PieceBehaviorDefinition[] m_pieceBehaviorDefinitions = Array.Empty<PieceBehaviorDefinition>();

    /// <inheritdoc />
    public IReadOnlyDictionary<int, string> GlyphIDMapping => InputSettings.GlyphIDMapping;

    /// <inheritdoc />
    public IReadOnlyCollection<IPieceBehaviorDefinition> PieceBehaviorDefinitions => m_pieceBehaviorDefinitions;
}
}
