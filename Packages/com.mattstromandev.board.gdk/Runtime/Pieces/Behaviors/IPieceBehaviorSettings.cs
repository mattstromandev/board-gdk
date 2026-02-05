namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Interface for settings used by the <see cref="PieceBehaviorSystem"/>.
/// </summary>
public interface IPieceBehaviorSettings
{
    // TODO: it could be nice to have a more generic way of creating new settings, since this code will be immutable
    // in the package, and currently new settings have to be added in code. We could instead use a dictionary of settings
    // keys to value types and desired values, but we'd ideally want tooling to try not to use string keys for improved
    // maintainability. This would also allow behaviors to override only individual settings rather than the whole set.
    // For now we'll stick with adding hard-coded settings here.
    
    /// <summary>
    /// Flag indicating whether piece settling is used when processing piece behavior. If true, behaviors will apply
    /// piece settling logic during their execution, which can help stabilize piece interactions and prevent jittery
    /// behavior.
    /// </summary>
    /// <remarks>
    /// Note that enabling piece settling will introduce a delay in behavior activation, which is adjustable via the
    /// <see cref="PieceSettlingFrames"/> setting. A higher value will provide more stability but will likely also result
    /// in a less responsive experience. If your game requires immediate behavior responses, consider disabling piece
    /// settling.
    /// </remarks>
    public bool UsePieceSettling { get; }

    /// <summary>
    /// The number of frames to wait for a piece to settle before processing its behavior. The behavior will not activate
    /// until the piece contact has persisted for this many frames. 
    /// </summary>
    public int PieceSettlingFrames { get; }
}
}
