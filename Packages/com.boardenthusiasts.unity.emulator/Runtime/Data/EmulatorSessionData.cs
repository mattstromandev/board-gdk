using System;
using System.Collections.Generic;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// Serializable emulator data describing the Board session state.
/// </summary>
[Serializable]
public sealed class EmulatorSessionData
{
    [SerializeField]
    [Tooltip("The Board player identifier that should be returned as the active profile.")]
    private string m_activeProfileId;

    [SerializeField]
    [Tooltip("The session players currently returned by the emulator. This is an emulator authoring convenience for seeding Board session state.")]
    private List<EmulatorSessionPlayerData> m_players = new();

    /// <summary>
    /// The Board player identifier that should be returned as the active profile.
    /// </summary>
    public string ActiveProfileId
    {
        get => m_activeProfileId;
        set => m_activeProfileId = value;
    }

    /// <summary>
    /// The session players currently returned by the emulator.
    /// </summary>
    public List<EmulatorSessionPlayerData> Players
    {
        get => m_players;
        set => m_players = value ?? new List<EmulatorSessionPlayerData>();
    }
}
}
