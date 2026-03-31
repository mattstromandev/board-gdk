using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using BE.Emulator.Data;

using UnityEngine;

namespace BE.Emulator.Utility
{
/// <summary>
/// Helpers for cloning and hashing emulator mock data.
/// </summary>
internal static class EmulatorMockDataUtility
{
    /// <summary>
    /// Creates a deep clone of the provided mock data.
    /// </summary>
    /// <param name="source">The source mock data to clone.</param>
    /// <returns>A deep clone of <paramref name="source"/>.</returns>
    public static EmulatorMockData Clone(EmulatorMockData source)
    {
        if(source == null)
        {
            return new EmulatorMockData();
        }

        return new EmulatorMockData
        {
            Application = new EmulatorApplicationData
            {
                IsProfileSwitcherVisible = source.Application?.IsProfileSwitcherVisible ?? false,
                HasPauseScreenContext = source.Application?.HasPauseScreenContext ?? false,
                ApplicationName = source.Application?.ApplicationName,
                ShowSaveOptionUponExit = source.Application?.ShowSaveOptionUponExit ?? false,
                CustomButtons = source.Application?.CustomButtons?.ToList() ?? new List<Board.Core.BoardPauseCustomButton>(),
                AudioTracks = source.Application?.AudioTracks?.ToList() ?? new List<Board.Core.BoardPauseAudioTrack>()
            },
            Profiles = source.Profiles?.Select(profile => new EmulatorProfileData
            {
                PlayerId = profile.PlayerId,
                DisplayName = profile.DisplayName,
                AvatarId = profile.AvatarId,
                Type = profile.Type,
                Avatar = profile.Avatar,
                AvatarBackgroundColor = profile.AvatarBackgroundColor
            }).ToList() ?? new List<EmulatorProfileData>(),
            Session = new EmulatorSessionData
            {
                ActiveProfileId = source.Session?.ActiveProfileId,
                Players = source.Session?.Players?.Select(player => new EmulatorSessionPlayerData
                {
                    SessionId = player.SessionId,
                    PlayerId = player.PlayerId,
                    Type = player.Type,
                    DisplayName = player.DisplayName,
                    AvatarId = player.AvatarId,
                    Avatar = player.Avatar,
                    AvatarBackgroundColor = player.AvatarBackgroundColor
                }).ToList() ?? new List<EmulatorSessionPlayerData>()
            },
            Storage = new EmulatorStorageData
            {
                MaxPayloadSize = source.Storage?.MaxPayloadSize ?? 16 * 1024 * 1024,
                MaxAppStorage = source.Storage?.MaxAppStorage ?? 64 * 1024 * 1024,
                MaxSaveDescriptionLength = source.Storage?.MaxSaveDescriptionLength ?? 100
            },
            SaveGames = source.SaveGames?.Select(saveGame => new EmulatorSaveGameData
            {
                SaveId = saveGame.SaveId,
                Description = saveGame.Description,
                CreatedAt = saveGame.CreatedAt,
                UpdatedAt = saveGame.UpdatedAt,
                PlayedTime = saveGame.PlayedTime,
                GameVersion = saveGame.GameVersion,
                CoverImage = saveGame.CoverImage,
                Payload = saveGame.Payload?.ToArray() ?? Array.Empty<byte>(),
                PayloadChecksum = saveGame.PayloadChecksum,
                CoverImageChecksum = saveGame.CoverImageChecksum,
                Players = saveGame.Players?.Select(player => new EmulatorSaveGamePlayerData
                {
                    PlayerId = player.PlayerId,
                    DisplayName = player.DisplayName,
                    AvatarId = player.AvatarId,
                    Type = player.Type,
                    Avatar = player.Avatar,
                    AvatarBackgroundColor = player.AvatarBackgroundColor
                }).ToList() ?? new List<EmulatorSaveGamePlayerData>()
            }).ToList() ?? new List<EmulatorSaveGameData>()
        };
    }

    /// <summary>
    /// Computes a SHA-256 hash for the provided binary payload.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The lowercase hexadecimal SHA-256 digest, or <see langword="null"/> when <paramref name="data"/> is null or empty.</returns>
    public static string ComputeSha256Hex(byte[] data)
    {
        if(data == null || data.Length == 0)
        {
            return null;
        }

        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(data);
        StringBuilder builder = new(hashBytes.Length * 2);
        foreach(byte value in hashBytes)
        {
            builder.Append(value.ToString("x2"));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Encodes a texture as PNG bytes for persistence and hashing.
    /// </summary>
    /// <param name="texture">The texture to encode.</param>
    /// <returns>The encoded PNG bytes, or <see langword="null"/> when <paramref name="texture"/> is null.</returns>
    public static byte[] EncodeTexture(Texture2D texture)
    {
        return texture != null ? ImageConversion.EncodeToPNG(texture) : null;
    }
}
}
