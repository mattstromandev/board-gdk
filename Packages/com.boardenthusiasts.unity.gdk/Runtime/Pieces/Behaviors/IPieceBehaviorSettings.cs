using UnityEngine;

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
    /// The axis in world space that is considered "up" for the purposes of piece behavior calculations.
    /// </summary>
    public Vector3 WorldUpAxis => Vector3.up;
}
}
