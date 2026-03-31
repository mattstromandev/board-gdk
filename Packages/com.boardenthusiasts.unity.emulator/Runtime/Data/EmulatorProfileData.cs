using System;

using Board.Core;

using UnityEngine;

namespace BE.Emulator.Data
{
/// <summary>
/// Serializable emulator data describing a Board profile.
/// </summary>
[Serializable]
public sealed class EmulatorProfileData
{
    [SerializeField]
    [Tooltip("The persistent Board player identifier returned for this mock profile.")]
    private string m_playerId;

    [SerializeField]
    [Tooltip("The display name shown for this mock profile.")]
    private string m_displayName;

    [SerializeField]
    [Tooltip("The avatar identifier returned for this mock profile.")]
    private string m_avatarId;

    [SerializeField]
    [Tooltip("The Board player type returned for this mock profile.")]
    private BoardPlayerType m_type = BoardPlayerType.Profile;

    [SerializeField]
    [Tooltip("The avatar texture returned when the Board SDK requests this mock profile's avatar.")]
    private Texture2D m_avatar;

    [SerializeField]
    [Tooltip("The internally persisted avatar background color used by the emulator UI for this profile.")]
    private Color m_avatarBackgroundColor;

    /// <summary>
    /// The persistent Board player identifier returned for this mock profile.
    /// </summary>
    public string PlayerId
    {
        get => m_playerId;
        set => m_playerId = value;
    }

    /// <summary>
    /// The display name shown for this mock profile.
    /// </summary>
    public string DisplayName
    {
        get => m_displayName;
        set => m_displayName = value;
    }

    /// <summary>
    /// The avatar identifier returned for this mock profile.
    /// </summary>
    public string AvatarId
    {
        get => m_avatarId;
        set => m_avatarId = value;
    }

    /// <summary>
    /// The Board player type returned for this mock profile.
    /// </summary>
    public BoardPlayerType Type
    {
        get => m_type;
        set => m_type = value;
    }

    /// <summary>
    /// The avatar texture returned for this mock profile.
    /// </summary>
    public Texture2D Avatar
    {
        get => m_avatar;
        set => m_avatar = value;
    }

    /// <summary>
    /// The internally persisted avatar background color used by the emulator UI for this profile.
    /// </summary>
    internal Color AvatarBackgroundColor
    {
        get => m_avatarBackgroundColor;
        set => m_avatarBackgroundColor = value;
    }
}
}
