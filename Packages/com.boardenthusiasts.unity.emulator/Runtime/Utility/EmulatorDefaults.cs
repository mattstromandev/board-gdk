using System.Collections.Generic;

using Board.Core;

using BE.Emulator.Data;

using UnityEngine;

namespace BE.Emulator.Utility
{
/// <summary>
/// Provides the built-in default mock data used when no persisted emulator data asset is available.
/// </summary>
public static class EmulatorDefaults
{
    /// <summary>
    /// Creates the default emulator mock data set.
    /// </summary>
    /// <returns>A populated default emulator mock data set.</returns>
    public static EmulatorMockData CreateMockData()
    {
        EmulatorProfileData commanderAvery = new()
        {
            PlayerId = "profile-avery",
            DisplayName = "Commander Avery",
            AvatarId = "0",
            Type = BoardPlayerType.Profile,
            Avatar = EmulatorAvatarUtility.CreateAvatarTexture(new Color(0.16f, 0.45f, 0.85f)),
            AvatarBackgroundColor = new Color(0.16f, 0.45f, 0.85f)
        };

        EmulatorProfileData captainNoor = new()
        {
            PlayerId = "profile-noor",
            DisplayName = "Captain Noor",
            AvatarId = "1",
            Type = BoardPlayerType.Profile,
            Avatar = EmulatorAvatarUtility.CreateAvatarTexture(new Color(0.85f, 0.41f, 0.17f)),
            AvatarBackgroundColor = new Color(0.85f, 0.41f, 0.17f)
        };

        return new EmulatorMockData
        {
            Profiles = new List<EmulatorProfileData> { commanderAvery, captainNoor },
            Session = new EmulatorSessionData
            {
                ActiveProfileId = commanderAvery.PlayerId,
                Players = new List<EmulatorSessionPlayerData>
                {
                    new()
                    {
                        SessionId = 1,
                        PlayerId = commanderAvery.PlayerId
                    }
                }
            },
            SaveGames = new List<EmulatorSaveGameData>(),
            Application = new EmulatorApplicationData(),
            Storage = new EmulatorStorageData()
        };
    }
}
}
