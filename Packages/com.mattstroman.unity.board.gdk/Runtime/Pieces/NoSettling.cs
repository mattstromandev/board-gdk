namespace BoardGDK.Pieces
{
/// <summary>
/// <see cref="IPieceSettlingStrategy"/> that considers all pieces settled immediately.
/// </summary>
public class NoSettling : IPieceSettlingStrategy
{
    /// <inheritdoc />
    public bool HasSettled(IPieceSettlingContext _)
    {
        return true;
    }
}
}
