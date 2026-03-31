using System;

using Board.Core;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// Serializable emulator data describing a player snapshot stored in a save game.
/// </summary>
[Serializable]
public sealed class EmulatorSaveGamePlayerData
{
    [SerializeField]
    [Tooltip("The Board player identifier returned for this saved player.")]
    private string m_playerId;

    [SerializeField]
    [Tooltip("The display name returned for this saved player.")]
    private string m_displayName;

    [SerializeField]
    [Tooltip("The avatar identifier returned for this saved player.")]
    private string m_avatarId;

    [SerializeField]
    [Tooltip("The Board player type returned for this saved player.")]
    private BoardPlayerType m_type = BoardPlayerType.Profile;

    [SerializeField]
    [Tooltip("The avatar texture returned for this saved player.")]
    private Texture2D m_avatar;

    [SerializeField]
    [Tooltip("The internally persisted avatar background color used by the emulator UI for this saved player.")]
    private Color m_avatarBackgroundColor;

    /// <summary>
    /// The Board player identifier returned for this saved player.
    /// </summary>
    public string PlayerId
    {
        get => m_playerId;
        set => m_playerId = value;
    }

    /// <summary>
    /// The display name returned for this saved player.
    /// </summary>
    public string DisplayName
    {
        get => m_displayName;
        set => m_displayName = value;
    }

    /// <summary>
    /// The avatar identifier returned for this saved player.
    /// </summary>
    public string AvatarId
    {
        get => m_avatarId;
        set => m_avatarId = value;
    }

    /// <summary>
    /// The Board player type returned for this saved player.
    /// </summary>
    public BoardPlayerType Type
    {
        get => m_type;
        set => m_type = value;
    }

    /// <summary>
    /// The avatar texture returned for this saved player.
    /// </summary>
    public Texture2D Avatar
    {
        get => m_avatar;
        set => m_avatar = value;
    }

    /// <summary>
    /// The internally persisted avatar background color used by the emulator UI for this saved player.
    /// </summary>
    internal Color AvatarBackgroundColor
    {
        get => m_avatarBackgroundColor;
        set => m_avatarBackgroundColor = value;
    }
}
}
