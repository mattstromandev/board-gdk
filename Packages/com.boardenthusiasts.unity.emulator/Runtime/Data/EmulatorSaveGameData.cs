using System;
using System.Collections.Generic;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// Serializable emulator data describing a save game entry.
/// </summary>
[Serializable]
public sealed class EmulatorSaveGameData
{
    [SerializeField]
    [HideInInspector]
    [Tooltip("The internal save identifier used by the emulator to look up this mock save.")]
    private string m_saveId;

    [SerializeField]
    [Tooltip("The save description returned for this mock save.")]
    [TextArea(2, 4)]
    private string m_description;

    [SerializeField]
    [HideInInspector]
    [Tooltip("The timestamp, in milliseconds, when this mock save was created.")]
    private ulong m_createdAt;

    [SerializeField]
    [HideInInspector]
    [Tooltip("The timestamp, in milliseconds, when this mock save was last updated.")]
    private ulong m_updatedAt;

    [SerializeField]
    [Tooltip("The played-time value, in seconds, returned for this mock save.")]
    private ulong m_playedTime;

    [SerializeField]
    [Tooltip("The game-version string returned for this mock save.")]
    private string m_gameVersion;

    [SerializeField]
    [Tooltip("The cover image returned for this mock save.")]
    private Texture2D m_coverImage;

    [SerializeField]
    [Tooltip("The payload bytes that will be returned when this mock save is loaded.")]
    private byte[] m_payload = Array.Empty<byte>();

    [SerializeField]
    [HideInInspector]
    [Tooltip("The checksum returned for this mock save payload.")]
    private string m_payloadChecksum;

    [SerializeField]
    [HideInInspector]
    [Tooltip("The checksum returned for this mock save cover image.")]
    private string m_coverImageChecksum;

    [SerializeField]
    [Tooltip("The player snapshot returned for this mock save. This is an emulator authoring convenience so saved-player metadata can be seeded directly.")]
    private List<EmulatorSaveGamePlayerData> m_players = new();

    /// <summary>
    /// The internal save identifier used by the emulator to look up this mock save.
    /// </summary>
    public string SaveId
    {
        get => m_saveId;
        set => m_saveId = value;
    }

    /// <summary>
    /// The save description returned for this mock save.
    /// </summary>
    public string Description
    {
        get => m_description;
        set => m_description = value;
    }

    /// <summary>
    /// The timestamp, in milliseconds, when this mock save was created.
    /// </summary>
    public ulong CreatedAt
    {
        get => m_createdAt;
        set => m_createdAt = value;
    }

    /// <summary>
    /// The timestamp, in milliseconds, when this mock save was last updated.
    /// </summary>
    public ulong UpdatedAt
    {
        get => m_updatedAt;
        set => m_updatedAt = value;
    }

    /// <summary>
    /// The played-time value, in seconds, returned for this mock save.
    /// </summary>
    public ulong PlayedTime
    {
        get => m_playedTime;
        set => m_playedTime = value;
    }

    /// <summary>
    /// The game-version string returned for this mock save.
    /// </summary>
    public string GameVersion
    {
        get => m_gameVersion;
        set => m_gameVersion = value;
    }

    /// <summary>
    /// The cover image returned for this mock save.
    /// </summary>
    public Texture2D CoverImage
    {
        get => m_coverImage;
        set => m_coverImage = value;
    }

    /// <summary>
    /// The payload bytes returned when this mock save is loaded.
    /// </summary>
    public byte[] Payload
    {
        get => m_payload;
        set => m_payload = value ?? Array.Empty<byte>();
    }

    /// <summary>
    /// The checksum returned for this mock save payload.
    /// </summary>
    public string PayloadChecksum
    {
        get => m_payloadChecksum;
        set => m_payloadChecksum = value;
    }

    /// <summary>
    /// The checksum returned for this mock save cover image.
    /// </summary>
    public string CoverImageChecksum
    {
        get => m_coverImageChecksum;
        set => m_coverImageChecksum = value;
    }

    /// <summary>
    /// The player snapshot returned for this mock save.
    /// </summary>
    public List<EmulatorSaveGamePlayerData> Players
    {
        get => m_players;
        set => m_players = value ?? new List<EmulatorSaveGamePlayerData>();
    }
}
}
