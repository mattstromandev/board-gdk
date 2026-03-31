using System;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// ScriptableObject asset that persists the emulator's mock Board OS state.
/// </summary>
[PreferBinarySerialization]
public sealed class EmulatorMockDataAsset : ScriptableObject
{
    [SerializeField]
    [Tooltip("The persisted mock Board data used by the Emulator in the Unity editor.")]
    private EmulatorMockData m_data = new();

    /// <summary>
    /// Raised when the persisted mock data changes.
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// The persisted mock Board data used by the Emulator.
    /// </summary>
    public EmulatorMockData Data => m_data;

    /// <summary>
    /// Replaces the persisted mock data and notifies listeners.
    /// </summary>
    /// <param name="data">The replacement mock data.</param>
    public void Replace(EmulatorMockData data)
    {
        m_data = data ?? new EmulatorMockData();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Notifies listeners that the persisted mock data has changed in-place.
    /// </summary>
    public void NotifyChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnValidate()
    {
        NotifyChanged();
    }
}
}
