using Board.Input;

namespace BoardGDK.Pieces
{
/// <summary>
/// Interface for strategies that determine how a piece settles on the board.
/// </summary>
public interface IPieceSettlingStrategy
{
    /// <summary>
    /// Determine if a <see cref="BoardContact"/> is considered settled according to this strategy.
    /// </summary>
    /// <param name="context">The <see cref="IPieceSettlingContext"/> providing necessary information for settling logic.</param>
    /// <returns>True if the contact has settled; false otherwise.</returns>
    public bool HasSettled(IPieceSettlingContext context);
}
}
