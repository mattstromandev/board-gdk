using System;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// Serializable emulator data describing save-system storage limits.
/// </summary>
[Serializable]
public sealed class EmulatorStorageData
{
    [SerializeField]
    [Tooltip("The maximum payload size, in bytes, returned by the Board save-game API.")]
    private long m_maxPayloadSize = 16 * 1024 * 1024;

    [SerializeField]
    [Tooltip("The total application storage, in bytes, returned by the Board save-game API.")]
    private long m_maxAppStorage = 64 * 1024 * 1024;

    [SerializeField]
    [Min(1)]
    [Tooltip("The maximum save-description length returned by the Board save-game API.")]
    private int m_maxSaveDescriptionLength = 100;

    /// <summary>
    /// The maximum payload size, in bytes, returned by the Board save-game API.
    /// </summary>
    public long MaxPayloadSize
    {
        get => m_maxPayloadSize;
        set => m_maxPayloadSize = value;
    }

    /// <summary>
    /// The total application storage, in bytes, returned by the Board save-game API.
    /// </summary>
    public long MaxAppStorage
    {
        get => m_maxAppStorage;
        set => m_maxAppStorage = value;
    }

    /// <summary>
    /// The maximum save-description length returned by the Board save-game API.
    /// </summary>
    public int MaxSaveDescriptionLength
    {
        get => m_maxSaveDescriptionLength;
        set => m_maxSaveDescriptionLength = value;
    }
}
}
