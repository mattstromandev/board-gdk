using Board.Input;

using BoardGDK.Pieces;
using BoardGDK.Utilities;

using UnityEngine;

namespace BoardGDK.Extensions.Pieces
{
/// <summary>
/// Extension methods for the <see cref="IVirtualPiece"/> interface.
/// </summary>
public static class VirtualPieceExtensions
{
    /// <summary>
    /// Retrieve the active <see cref="BoardContact"/> for this <see cref="IVirtualPiece"/>.
    /// </summary>
    /// <returns>The <see cref="BoardContact"/> for this <see cref="IVirtualPiece"/>, if it is active; null otherwise.</returns>
    public static BoardContact GetBoardContact(this IVirtualPiece me)
    {
        return BoardInputUtil.GetBoardContactByID(me.BoardContactID);
    }
    
    /// <summary>
    /// Determine if the associated physical Piece is actively being touched.
    /// </summary>
    /// <returns>True if the physical Piece is actively being touched; false otherwise.</returns>
    public static bool IsTouched(this IVirtualPiece me)
    {
        return me.GetBoardContact().isTouched;
    }

    /// <summary>
    /// Determine if the Piece associated with the given <see cref="IVirtualPiece"/> is within range of a target world position from the perspective of a given <see cref="Camera"/>.
    /// </summary>
    /// <param name="targetWorldPosition">The target position in world space.</param>
    /// <param name="range">The allowed range, in world units.</param>
    /// <param name="camera">The <see cref="Camera"/> to use for perspective.</param>
    /// <returns>True if the associated Piece is within range of the <paramref name="targetWorldPosition"/>; false otherwise.</returns>
    public static bool IsPieceInRange(this VirtualPiece me, Vector3 targetWorldPosition, float range, Camera camera)
    {
        BoardContact pieceBoardContact = me.GetBoardContact();

        if(pieceBoardContact.isNoneEndedOrCanceled) { return false; }

        Vector3 targetScreenPosition = camera.WorldToScreenPoint(targetWorldPosition);
        Vector3 boardContactScreenPosition = pieceBoardContact.screenPosition;

        // Convert the screen position of the piece to a world position
        Vector3 pieceWorldPosition = camera.ScreenToWorldPoint(new Vector3(boardContactScreenPosition.x
          , boardContactScreenPosition.y, targetScreenPosition.z));
        pieceWorldPosition.y = targetWorldPosition.y;

        return Vector3.Distance(pieceWorldPosition, targetWorldPosition) <= range;
    }
}
}
