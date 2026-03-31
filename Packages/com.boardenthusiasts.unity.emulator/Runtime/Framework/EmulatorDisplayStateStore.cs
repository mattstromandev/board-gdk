using System;
using System.Collections.Concurrent;

namespace BE.Emulator.Framework
{
/// <inheritdoc cref="IEmulatorDisplayStateStore"/>
public sealed class EmulatorDisplayStateStore : IEmulatorDisplayStateStore
{
    private readonly ConcurrentDictionary<Type, bool> _visibilityByType = new();

    /// <inheritdoc />
    public bool IsVisible(Type displayType)
    {
        if(displayType == null)
        {
            throw new ArgumentNullException(nameof(displayType));
        }

        return _visibilityByType.TryGetValue(displayType, out bool isVisible) && isVisible;
    }

    /// <inheritdoc />
    public bool IsVisible<TDisplay>() where TDisplay : IEmulatorDisplay
    {
        return IsVisible(typeof(TDisplay));
    }

    /// <inheritdoc />
    public void SetVisible(Type displayType, bool isVisible)
    {
        if(displayType == null)
        {
            throw new ArgumentNullException(nameof(displayType));
        }

        _visibilityByType.AddOrUpdate(displayType, isVisible, (_, _) => isVisible);
    }
}
}
