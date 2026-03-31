using System;

using Board.Core;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// Serializable emulator data describing a player assigned to the current session.
/// </summary>
[Serializable]
public sealed class EmulatorSessionPlayerData
{
    [SerializeField]
    [Min(1)]
    [Tooltip("The session identifier returned for this mock session player.")]
    private int m_sessionId = 1;

    [SerializeField]
    [Tooltip("The Board player identifier mapped to this mock session slot.")]
    private string m_playerId;

    [SerializeField]
    [Tooltip("The player type returned for this mock session slot. Guest players may exist only in the session without a backing profile.")]
    private BoardPlayerType m_type = BoardPlayerType.Profile;

    [SerializeField]
    [Tooltip("The display name returned for this mock session slot when it is a session-only Guest player.")]
    private string m_displayName;

    [SerializeField]
    [Tooltip("The avatar identifier returned for this mock session slot when it is a session-only Guest player.")]
    private string m_avatarId;

    [SerializeField]
    [Tooltip("The avatar texture returned for this mock session slot when it is a session-only Guest player.")]
    private Texture2D m_avatar;

    [SerializeField]
    [Tooltip("The internally persisted avatar background color for this mock session slot when it is a session-only Guest player.")]
    private Color m_avatarBackgroundColor;

    /// <summary>
    /// The session identifier returned for this mock session player.
    /// </summary>
    public int SessionId
    {
        get => m_sessionId;
        set => m_sessionId = value;
    }

    /// <summary>
    /// The Board player identifier mapped to this mock session slot.
    /// </summary>
    public string PlayerId
    {
        get => m_playerId;
        set => m_playerId = value;
    }

    /// <summary>
    /// The player type returned for this mock session slot.
    /// </summary>
    public BoardPlayerType Type
    {
        get => m_type;
        set => m_type = value;
    }

    /// <summary>
    /// The display name returned for this mock session slot when it is a session-only guest.
    /// </summary>
    public string DisplayName
    {
        get => m_displayName;
        set => m_displayName = value;
    }

    /// <summary>
    /// The avatar identifier returned for this mock session slot when it is a session-only guest.
    /// </summary>
    public string AvatarId
    {
        get => m_avatarId;
        set => m_avatarId = value;
    }

    /// <summary>
    /// The avatar texture returned for this mock session slot when it is a session-only guest.
    /// </summary>
    public Texture2D Avatar
    {
        get => m_avatar;
        set => m_avatar = value;
    }

    /// <summary>
    /// The internally persisted avatar background color for this mock session slot when it is a session-only guest.
    /// </summary>
    internal Color AvatarBackgroundColor
    {
        get => m_avatarBackgroundColor;
        set => m_avatarBackgroundColor = value;
    }
}
}
