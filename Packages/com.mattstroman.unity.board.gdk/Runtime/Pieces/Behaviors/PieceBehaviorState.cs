using System.Collections.Generic;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Default implementation of an <see cref="IPieceBehaviorState"/>.
/// </summary>
public class PieceBehaviorState : IPieceBehaviorState
{
    private readonly Dictionary<string, object> _stateStore = new();
    
    /// <inheritdoc />
    public bool TryGetValue<T>(string key, out T value)
    {
        if(_stateStore.TryGetValue(key, out object storedValue) && storedValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default(T);
        return false;
    }

    /// <inheritdoc />
    public void SetValue<T>(string key, T value)
    {
        _stateStore[key] = value;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _stateStore.Clear();
    }
}
}
