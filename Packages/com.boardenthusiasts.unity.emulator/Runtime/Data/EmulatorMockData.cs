using System;
using System.Collections.Generic;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// Root serializable data set backing the emulator's mock Board OS state.
/// </summary>
[Serializable]
public sealed class EmulatorMockData
{
    [SerializeField]
    [Tooltip("The mock Board application state returned by the emulator.")]
    private EmulatorApplicationData m_application = new();

    [SerializeField]
    [Tooltip("The mock Board profiles available to the emulator.")]
    private List<EmulatorProfileData> m_profiles = new();

    [SerializeField]
    [Tooltip("The mock Board session state returned by the emulator.")]
    private EmulatorSessionData m_session = new();

    [SerializeField]
    [Tooltip("The mock Board storage limits returned by the emulator.")]
    private EmulatorStorageData m_storage = new();

    [SerializeField]
    [Tooltip("The mock Board save games returned by the emulator.")]
    private List<EmulatorSaveGameData> m_saveGames = new();

    /// <summary>
    /// The mock Board application state returned by the emulator.
    /// </summary>
    public EmulatorApplicationData Application
    {
        get => m_application;
        set => m_application = value ?? new EmulatorApplicationData();
    }

    /// <summary>
    /// The mock Board profiles available to the emulator.
    /// </summary>
    public List<EmulatorProfileData> Profiles
    {
        get => m_profiles;
        set => m_profiles = value ?? new List<EmulatorProfileData>();
    }

    /// <summary>
    /// The mock Board session state returned by the emulator.
    /// </summary>
    public EmulatorSessionData Session
    {
        get => m_session;
        set => m_session = value ?? new EmulatorSessionData();
    }

    /// <summary>
    /// The mock Board storage limits returned by the emulator.
    /// </summary>
    public EmulatorStorageData Storage
    {
        get => m_storage;
        set => m_storage = value ?? new EmulatorStorageData();
    }

    /// <summary>
    /// The mock Board save games returned by the emulator.
    /// </summary>
    public List<EmulatorSaveGameData> SaveGames
    {
        get => m_saveGames;
        set => m_saveGames = value ?? new List<EmulatorSaveGameData>();
    }
}
}
