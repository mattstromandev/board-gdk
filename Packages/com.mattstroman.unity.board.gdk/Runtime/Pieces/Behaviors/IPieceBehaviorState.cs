namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Interface representing the state of a piece behavior, which can be used to store information across activations and
/// updates of the behavior.
/// </summary>
public interface IPieceBehaviorState
{
    /// <summary>
    /// Get a value from the behavior state.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="value">The stored value of type <typeparamref name="T"/>, if found; <see langword="default"/> otherwise.</param>
    /// <typeparam name="T">The type of value expected to be stored.</typeparam>
    /// <returns>True if a value with <paramref name="key"/> was found; false otherwise.</returns>
    public bool TryGetValue<T>(string key, out T value);
    
    /// <summary>
    /// Set a value in the behavior state.
    /// </summary>
    /// <param name="key">The key of the value to set.</param>
    /// <param name="value">The value to store.</param>
    /// <typeparam name="T">The type of value to store.</typeparam>
    public void SetValue<T>(string key, T value);

    /// <summary>
    /// Clear the state.
    /// </summary>
    public void Clear();
}
}
