using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

using Board.Core;
using Board.Save;
using Board.Session;

using BE.Emulator.Data;

namespace BE.Emulator.Utility
{
/// <summary>
/// Creates Board SDK runtime objects from emulator persistence data.
/// </summary>
internal sealed class BoardSdkObjectFactory
{
    private static readonly BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static UnityEngine.Texture2D _fallbackAvatarTexture;

    /// <summary>
    /// Creates a Board SDK player from emulator profile data.
    /// </summary>
    /// <param name="profile">The profile data to project.</param>
    /// <returns>The projected Board player, or <see langword="null"/> when <paramref name="profile"/> is null.</returns>
    public BoardPlayer CreateBoardPlayer(EmulatorProfileData profile)
    {
        if(profile == null)
        {
            return null;
        }

        BoardPlayer player = (BoardPlayer)FormatterServices.GetUninitializedObject(typeof(BoardPlayer));
        PopulateBoardPlayer(player, profile.PlayerId, profile.DisplayName, profile.AvatarId, profile.Type, profile.Avatar, profile.AvatarBackgroundColor);
        return player;
    }

    /// <summary>
    /// Creates a Board SDK session player from emulator session and profile data.
    /// </summary>
    /// <param name="sessionPlayer">The session player data to project.</param>
    /// <param name="profile">The profile data for the session player.</param>
    /// <returns>The projected Board session player, or <see langword="null"/> when the input data is incomplete.</returns>
    public BoardSessionPlayer CreateSessionPlayer(EmulatorSessionPlayerData sessionPlayer, EmulatorProfileData profile)
    {
        if(sessionPlayer == null || profile == null)
        {
            return null;
        }

        BoardSessionPlayer player = (BoardSessionPlayer)FormatterServices.GetUninitializedObject(typeof(BoardSessionPlayer));
        PopulateBoardPlayer(player, profile.PlayerId, profile.DisplayName, profile.AvatarId, profile.Type, profile.Avatar, profile.AvatarBackgroundColor);
        SetBackingField(player, "<playerId>k__BackingField", profile.PlayerId);
        SetBackingField(player, "<sessionId>k__BackingField", sessionPlayer.SessionId);
        return player;
    }

    /// <summary>
    /// Creates a Board SDK save-game player snapshot from emulator save-game player data.
    /// </summary>
    /// <param name="profile">The save-game player data to project.</param>
    /// <returns>The projected save-game player, or <see langword="null"/> when <paramref name="profile"/> is null.</returns>
    public BoardSaveGamePlayer CreateSaveGamePlayer(EmulatorSaveGamePlayerData profile)
    {
        if(profile == null)
        {
            return null;
        }

        BoardSaveGamePlayer player = (BoardSaveGamePlayer)FormatterServices.GetUninitializedObject(typeof(BoardSaveGamePlayer));
        PopulateBoardPlayer(player, profile.PlayerId, profile.DisplayName, profile.AvatarId, profile.Type, profile.Avatar, profile.AvatarBackgroundColor);
        SetBackingField(player, "<playerId>k__BackingField", profile.PlayerId);
        return player;
    }

    /// <summary>
    /// Creates Board SDK save-game metadata from emulator save-game data.
    /// </summary>
    /// <param name="saveGame">The save-game data to project.</param>
    /// <returns>The projected save-game metadata, or <see langword="null"/> when <paramref name="saveGame"/> is null.</returns>
    public BoardSaveGameMetadata CreateSaveGameMetadata(EmulatorSaveGameData saveGame)
    {
        if(saveGame == null)
        {
            return null;
        }

        BoardSaveGameMetadata metadata = new();
        SetProperty(metadata, nameof(BoardSaveGameMetadata.id), saveGame.SaveId);
        SetProperty(metadata, nameof(BoardSaveGameMetadata.description), saveGame.Description);
        SetProperty(metadata, nameof(BoardSaveGameMetadata.createdAt), saveGame.CreatedAt);
        SetProperty(metadata, nameof(BoardSaveGameMetadata.updatedAt), saveGame.UpdatedAt);
        SetProperty(metadata, nameof(BoardSaveGameMetadata.playedTime), saveGame.PlayedTime);
        SetProperty(metadata, nameof(BoardSaveGameMetadata.gameVersion), saveGame.GameVersion);
        SetProperty(metadata, nameof(BoardSaveGameMetadata.hasCoverImage), saveGame.CoverImage != null);
        SetProperty(metadata, nameof(BoardSaveGameMetadata.payloadChecksum), saveGame.PayloadChecksum);
        SetProperty(metadata, nameof(BoardSaveGameMetadata.coverImageChecksum), saveGame.CoverImageChecksum);

        BoardSaveGamePlayer[] players = saveGame.Players?.Select(CreateSaveGamePlayer).Where(player => player != null).ToArray()
            ?? Array.Empty<BoardSaveGamePlayer>();
        SetProperty(metadata, nameof(BoardSaveGameMetadata.playerIds), players.Select(player => player.playerId).ToArray());
        SetProperty(metadata, nameof(BoardSaveGameMetadata.players), players);
        return metadata;
    }

    /// <summary>
    /// Creates a Board SDK pause screen context from emulator application data.
    /// </summary>
    /// <param name="application">The application data to project.</param>
    /// <param name="defaultApplicationName">The fallback application name.</param>
    /// <returns>The projected pause screen context.</returns>
    public BoardPauseScreenContext CreatePauseScreenContext(
        EmulatorApplicationData application,
        string defaultApplicationName)
    {
        application ??= new EmulatorApplicationData();
        return new BoardPauseScreenContext
        {
            applicationName = application.ApplicationName ?? defaultApplicationName,
            showSaveOptionUponExit = application.ShowSaveOptionUponExit,
            customButtons = application.CustomButtons?.ToArray() ?? Array.Empty<BoardPauseCustomButton>(),
            audioTracks = application.AudioTracks?.ToArray() ?? Array.Empty<BoardPauseAudioTrack>()
        };
    }

    private static void PopulateBoardPlayer(
        BoardPlayer player,
        string playerId,
        string displayName,
        string avatarId,
        BoardPlayerType type,
        UnityEngine.Texture2D avatar,
        UnityEngine.Color avatarBackgroundColor)
    {
        string normalizedAvatarId = int.TryParse(avatarId, out _) ? avatarId : "0";
        SetProperty(player, nameof(BoardPlayer.playerId), playerId);
        SetProperty(player, nameof(BoardPlayer.name), NormalizeDisplayName(displayName, playerId));
        SetProperty(player, nameof(BoardPlayer.avatarId), normalizedAvatarId);
        SetProperty(player, nameof(BoardPlayer.type), type);
        player.avatar = avatar ?? CreateGeneratedAvatarTexture(avatarBackgroundColor) ?? GetFallbackAvatarTexture();

        if(player.GetType() == typeof(BoardPlayer))
        {
            SetBackingField(player, "<logTag>k__BackingField", nameof(BoardPlayer));
        }
    }

    private static string NormalizeDisplayName(string displayName, string playerId)
    {
        if(string.IsNullOrWhiteSpace(displayName) == false)
        {
            return displayName;
        }

        return string.IsNullOrWhiteSpace(playerId) ? "Player" : playerId;
    }

    private static void SetBackingField(object instance, string fieldName, object value)
    {
        FieldInfo field = FindField(instance.GetType(), fieldName);
        field?.SetValue(instance, value);
    }

    private static void SetField(object instance, string fieldName, object value)
    {
        FieldInfo field = FindField(instance.GetType(), fieldName);
        field?.SetValue(instance, value);
    }

    private static void SetProperty(object instance, string propertyName, object value)
    {
        PropertyInfo property = FindProperty(instance.GetType(), propertyName);
        property?.SetValue(instance, value);
    }

    private static FieldInfo FindField(Type type, string fieldName)
    {
        while(type != null)
        {
            FieldInfo field = type.GetField(fieldName, InstanceBindingFlags);
            if(field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static UnityEngine.Texture2D CreateGeneratedAvatarTexture(UnityEngine.Color avatarBackgroundColor)
    {
        if(EmulatorAvatarUtility.HasSerializedColor(avatarBackgroundColor) == false)
        {
            return null;
        }

        return EmulatorAvatarUtility.CreateAvatarTexture(avatarBackgroundColor, hideAndDontSave: true);
    }

    private static PropertyInfo FindProperty(Type type, string propertyName)
    {
        while(type != null)
        {
            PropertyInfo property = type.GetProperty(propertyName, InstanceBindingFlags);
            if(property != null)
            {
                return property;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static UnityEngine.Texture2D GetFallbackAvatarTexture()
    {
        if(_fallbackAvatarTexture != null)
        {
            return _fallbackAvatarTexture;
        }

        _fallbackAvatarTexture = new UnityEngine.Texture2D(4, 4, UnityEngine.TextureFormat.RGBA32, false)
        {
            name = "EmulatorFallbackAvatar",
            hideFlags = UnityEngine.HideFlags.HideAndDontSave
        };

        UnityEngine.Color fallbackColor = new(0.76f, 0.80f, 0.84f, 1f);
        UnityEngine.Color[] pixels = Enumerable.Repeat(fallbackColor, 16).ToArray();
        _fallbackAvatarTexture.SetPixels(pixels);
        _fallbackAvatarTexture.Apply();
        return _fallbackAvatarTexture;
    }
}
}
