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
        if(me.TryGetPiecesOnBoard(glyphID, out VirtualPiece[] pieces) == false) { return false; }
        
        foreach(VirtualPiece piece in pieces)
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
        if(me.TryGetPiecesOnBoard(pieceBehaviorDefinition, out VirtualPiece[] pieces) == false) { return false; }

        foreach(VirtualPiece piece in pieces)
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
