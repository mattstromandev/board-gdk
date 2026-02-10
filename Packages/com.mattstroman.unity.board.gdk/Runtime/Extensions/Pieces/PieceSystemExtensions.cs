using System.Linq;

using BoardGDK.Pieces;
using BoardGDK.Pieces.Behaviors;

using UnityEngine;

namespace BoardGDK.Extensions.Pieces
{
/// <summary>
/// Extension methods for the <see cref="IPieceSystem"/> interface.
/// </summary>
public static class PieceSystemExtensions
{
    /// <summary>
    /// Determine if there is a Piece with the specified <paramref name="glyphID"/> on Board that is actively being touched.
    /// </summary>
    /// <param name="glyphID">The glyph ID to query.</param>
    /// <returns>True if there is at least one Piece with matching <paramref name="glyphID"/> on Board that is actively being touched; false otherwise.</returns>
    public static bool IsTouched(this IPieceSystem me, int glyphID)
    {
        bool isPieceOnBoard = me.TryGetPiecesOnBoard(glyphID, out IVirtualPiece[] pieces);
        
        if(isPieceOnBoard == false) { return false; }
        
        return pieces.Any(piece => piece.IsTouched());
    }

    /// <summary>
    /// Determine if there is a Piece matching the specified <paramref name="pieceBehaviorDefinition"/> on Board that is actively being touched.
    /// </summary>
    /// <param name="pieceBehaviorDefinition">The <see cref="PieceBehaviorDefinition"/> to match against.</param>
    /// <returns>True if there is at least one Piece matching the <paramref name="pieceBehaviorDefinition"/> on Board that is actively being touched; false otherwise.</returns>
    public static bool IsTouched(this IPieceSystem me, PieceBehaviorDefinition pieceBehaviorDefinition)
    {
        bool isPieceOnBoard = me.TryGetPiecesOnBoard(pieceBehaviorDefinition, out IVirtualPiece[] pieces);
        
        if(isPieceOnBoard == false) { return false; }
        
        return pieces.Any(piece => piece.IsTouched());
    }
    
    /// <summary>
    /// Determine if there is a Piece with the specified <paramref name="glyphID"/> on Board.
    /// </summary>
    /// <param name="glyphID">The glyph ID to query.</param>
    /// <returns>True if there is at least one Piece with matching <paramref name="glyphID"/> on Board; false otherwise.</returns>
    public static bool IsOnBoard(this IPieceSystem me, int glyphID) { return me.TryGetPiecesOnBoard(glyphID, out _); }

    /// <summary>
    /// Determine if there is a Piece matching the specified <paramref name="pieceBehaviorDefinition"/> on Board.
    /// </summary>
    /// <param name="pieceBehaviorDefinition">The <see cref="PieceBehaviorDefinition"/> to match against.</param>
    /// <returns>True if there is at least one Piece matching the <paramref name="pieceBehaviorDefinition"/> on Board; false otherwise.</returns>
    public static bool IsOnBoard(this IPieceSystem me, PieceBehaviorDefinition pieceBehaviorDefinition)
    {
        return me.TryGetPiecesOnBoard(pieceBehaviorDefinition, out _);
    }
    
    /// <summary>
    /// Determine if any Pieces with the specified glyph ID are within range of a target world position.
    /// </summary>
    /// <param name="glyphID">The glyph ID to query.</param>
    /// <param name="targetWorldPosition">The target position in world space.</param>
    /// <param name="range">The allowed range.</param>
    /// <returns>True if there is a matching Piece within range of the <paramref name="targetWorldPosition"/>; false otherwise.</returns>
    public static bool IsPieceInRange(this IPieceSystem me, int glyphID, Vector3 targetWorldPosition, float range)
    {
        return me.IsPieceInRange(glyphID, targetWorldPosition, range, Camera.main);
    }

    /// <summary>
    /// Determine if any Pieces with the specified glyph ID are within range of a target world position from the perspective of a given <see cref="Camera"/>.
    /// </summary>
    /// <param name="glyphID">The glyph ID to query.</param>
    /// <param name="targetWorldPosition">The target position in world space.</param>
    /// <param name="range">The allowed range.</param>
    /// <param name="camera">The <see cref="Camera"/> to use for perspective.</param>
    /// <returns>True if there is a matching Piece within range of the <paramref name="targetWorldPosition"/>; false otherwise.</returns>
    public static bool IsPieceInRange(this IPieceSystem me, int glyphID, Vector3 targetWorldPosition, float range, Camera camera)
    {
        if(me.TryGetPiecesOnBoard(glyphID, out IVirtualPiece[] pieces) == false) { return false; }
        
        foreach(IVirtualPiece piece in pieces)
        {
            if(piece.IsPieceInRange(targetWorldPosition, range, camera))
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Determine if any Pieces matching the specified behavior are within range of a target world position.
    /// </summary>
    /// <param name="pieceBehaviorDefinition">The <see cref="PieceBehaviorDefinition"/> to match against.</param>
    /// <param name="targetWorldPosition">The target position in world space.</param>
    /// <param name="range">The allowed range, in world units.</param>
    /// <returns>True if there is a matching Piece within range of the <paramref name="targetWorldPosition"/>; false otherwise.</returns>
    public static bool IsPieceInRange(this IPieceSystem me, PieceBehaviorDefinition pieceBehaviorDefinition, Vector3 targetWorldPosition, float range)
    {
        return me.IsPieceInRange(pieceBehaviorDefinition, targetWorldPosition, range, Camera.main);
    }

    /// <summary>
    /// Determine if any Pieces matching the specified behavior are within range of a target world position from the perspective of a given <see cref="Camera"/>.
    /// </summary>
    /// <param name="pieceBehaviorDefinition">The <see cref="PieceBehaviorDefinition"/> to match against.</param>
    /// <param name="targetWorldPosition">The target position in world space.</param>
    /// <param name="range">The allowed range, in world units.</param>
    /// <param name="camera">The <see cref="Camera"/> to use for perspective.</param>
    /// <returns>True if there is a matching Piece within range of the <paramref name="targetWorldPosition"/>; false otherwise.</returns>
    public static bool IsPieceInRange(this IPieceSystem me, PieceBehaviorDefinition pieceBehaviorDefinition, Vector3 targetWorldPosition, float range, Camera camera)
    {
        if(me.TryGetPiecesOnBoard(pieceBehaviorDefinition, out IVirtualPiece[] pieces) == false) { return false; }

        foreach(IVirtualPiece piece in pieces)
        {
            if(piece.IsPieceInRange(targetWorldPosition, range, camera))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determine if the Piece associated with the given <see cref="VirtualPiece"/> is within range of a target world position.
    /// </summary>
    /// <param name="virtualPiece">The <see cref="VirtualPiece"/> to check.</param>
    /// <param name="targetWorldPosition">The target position in world space.</param>
    /// <param name="range">The allowed range, in world units.</param>
    /// <returns>True if the associated Piece is within range of the <paramref name="targetWorldPosition"/>; false otherwise.</returns>
    public static bool IsPieceInRange(this IPieceSystem me, VirtualPiece virtualPiece, Vector3 targetWorldPosition, float range)
    {
        return virtualPiece.IsPieceInRange(targetWorldPosition, range, Camera.main);
    }
}
}
