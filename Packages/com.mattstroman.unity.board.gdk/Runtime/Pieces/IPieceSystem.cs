using System;

using Board.Input;

using BoardGDK.Pieces.Behaviors;

using JetBrains.Annotations;

namespace BoardGDK.Pieces
{
/// <summary>
/// Interface for a system that manages state of virtual pieces on Board.
/// </summary>
public interface IPieceSystem
{
    /// <summary>
    /// The currently active <see cref="IPieceSetDefinition"/>.
    /// </summary>
    [CanBeNull]
    public IPieceSetDefinition ActivePieceSetDefinition { get; }
    
    /// <summary>
    /// Notification event invoked when the active Piece set has changed.
    /// </summary>
    public event Action<IPieceSetDefinition> PieceSetChanged;
    
    /// <summary>
    /// Change the active Piece set.
    /// </summary>
    /// <remarks>
    /// <para>Note that switching the active Piece set will cause a reset of all currently active Pieces on Board.</para>
    /// <para>
    /// Additionally, this invokes a change of the <see cref="BoardInput.settings"/> which takes a significant amount of
    /// time to process. It is recommended to avoid switching Piece sets during active gameplay, and to do so only
    /// during loading screens or other natural breaks in gameplay.
    /// </para>
    /// </remarks>
    /// <param name="newPieceSet"></param>
    /// <seealso cref="BoardInput.settings"/>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="newPieceSet"/> is null.</exception>
    public void ChangePieceSet([NotNull] IPieceSetDefinition newPieceSet);
    
    /// <summary>
    /// Determine if there are any Pieces with the specified <paramref name="glyphID"/> on Board, and retrieve them.
    /// </summary>
    /// <param name="glyphID">The glyph ID to query.</param>
    /// <param name="pieces">The set of matching <see cref="VirtualPiece"/>s, if there are any; empty array otherwise.</param>
    /// <returns>True if there is at least one Piece with matching <paramref name="glyphID"/> on Board; false otherwise.</returns>
    public bool TryGetPiecesOnBoard(int glyphID, [NotNull] out IVirtualPiece[] pieces);

    /// <summary>
    /// Determine if there are any Pieces matching the specified <paramref name="pieceBehaviorDefinition"/> on Board, and retrieve them.
    /// </summary>
    /// <param name="pieceBehaviorDefinition">The <see cref="PieceBehaviorDefinition"/> to match against.</param>
    /// <param name="pieces">The set of matching <see cref="VirtualPiece"/>s, if there are any; empty array otherwise.</param>
    /// <returns>True if there is at least one Piece matching the <paramref name="pieceBehaviorDefinition"/> on Board; false otherwise.</returns>
    public bool TryGetPiecesOnBoard([NotNull] PieceBehaviorDefinition pieceBehaviorDefinition, [NotNull] out IVirtualPiece[] pieces);
}
}
