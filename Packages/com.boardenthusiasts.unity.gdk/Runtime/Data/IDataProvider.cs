namespace BoardGDK.Data
{
/// <summary>
/// Interface for providers of data.
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Retrieve a piece of data by its key.
    /// </summary>
    /// <param name="key">The key of the data to retrieve.</param>
    /// <param name="data">The current value of the data, if found; default otherwise.</param>
    /// <typeparam name="T">The type of data to retrieve.</typeparam>
    /// <returns>True if the data with <paramref name="key"/> exists; false otherwise.</returns>
    public bool TryGetData<T>(string key, out T data);
    
    /// <summary>
    /// Set a piece of data by its key.
    /// </summary>
    /// <param name="key">The key of the data to set.</param>
    /// <param name="data">The value of the data to set.</param>
    /// <typeparam name="T">The type of data to set.</typeparam>
    public void SetData<T>(string key, T data);
}
}
