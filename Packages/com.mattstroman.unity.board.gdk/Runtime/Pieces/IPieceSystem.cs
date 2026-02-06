using System;

using Board.Input;

using BoardGDK.Pieces.Behaviors;

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
    public void ChangePieceSet(IPieceSetDefinition newPieceSet);
    
    /// <summary>
    /// Determine if there is a Piece with the specified <paramref name="glyphID"/> on Board.
    /// </summary>
    /// <param name="glyphID">The glyph ID to query.</param>
    /// <returns>True if there is at least one Piece with matching <paramref name="glyphID"/> on Board; false otherwise.</returns>
    public bool IsOnBoard(int glyphID);

    /// <summary>
    /// Determine if there is a Piece matching the specified <paramref name="pieceBehaviorDefinition"/> on Board.
    /// </summary>
    /// <param name="pieceBehaviorDefinition">The <see cref="PieceBehaviorDefinition"/> to match against.</param>
    /// <returns>True if there is at least one Piece matching the <paramref name="pieceBehaviorDefinition"/> on Board; false otherwise.</returns>
    public bool IsOnBoard(PieceBehaviorDefinition pieceBehaviorDefinition);
    
    /// <summary>
    /// Determine if there is a Piece with the specified <paramref name="glyphID"/> on Board that is actively being touched.
    /// </summary>
    /// <param name="glyphID">The glyph ID to query.</param>
    /// <returns>True if there is at least one Piece with matching <paramref name="glyphID"/> on Board that is actively being touched; false otherwise.</returns>
    public bool IsTouched(int glyphID);
    
    /// <summary>
    /// Determine if there is a Piece matching the specified <paramref name="pieceBehaviorDefinition"/> on Board that is actively being touched.
    /// </summary>
    /// <param name="pieceBehaviorDefinition">The <see cref="PieceBehaviorDefinition"/> to match against.</param>
    /// <returns>True if there is at least one Piece matching the <paramref name="pieceBehaviorDefinition"/> on Board that is actively being touched; false otherwise.</returns>
    public bool IsTouched(PieceBehaviorDefinition pieceBehaviorDefinition);
    
    /// <summary>
    /// Determine if there are any Pieces with the specified <paramref name="glyphID"/> on Board, and retrieve them.
    /// </summary>
    /// <param name="glyphID">The glyph ID to query.</param>
    /// <param name="pieces">The set of matching <see cref="VirtualPiece"/>s, if there are any.</param>
    /// <returns>True if there is at least one Piece with matching <paramref name="glyphID"/> on Board; false otherwise.</returns>
    public bool TryGetPiecesOnBoard(int glyphID, out VirtualPiece[] pieces);

    /// <summary>
    /// Determine if there are any Pieces matching the specified <paramref name="pieceBehaviorDefinition"/> on Board, and retrieve them.
    /// </summary>
    /// <param name="pieceBehaviorDefinition">The <see cref="PieceBehaviorDefinition"/> to match against.</param>
    /// <param name="pieces">The set of matching <see cref="VirtualPiece"/>s, if there are any.</param>
    /// <returns>True if there is at least one Piece matching the <paramref name="pieceBehaviorDefinition"/> on Board; false otherwise.</returns>
    public bool TryGetPiecesOnBoard(PieceBehaviorDefinition pieceBehaviorDefinition, out VirtualPiece[] pieces);
}
}
