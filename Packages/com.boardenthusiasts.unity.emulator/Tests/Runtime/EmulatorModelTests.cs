using System;
using System.Linq;
using System.Threading.Tasks;

using Board.Core;
using Board.Save;

using BE.Emulator.Data;
using BE.Emulator.Persistence;
using BE.Emulator.Utility;

using NUnit.Framework;

using Rahmen.Logging;

using UnityEngine;

namespace BE.Emulator.Tests
{
public sealed class EmulatorModelTests
{
    [SetUp]
    public void SetUp()
    {
        EmulatorProjectSettingsBridge.SetActiveMockDataAsset(null);
    }

    [Test]
    public void ApplicationState_RoundTripsThroughModel()
    {
        EmulatorModel model = CreateModel();

        model.ShowProfileSwitcher();
        model.SetPauseScreenContext(
            applicationName: "Naval Warfare",
            showSaveOptionUponExit: true,
            customButtons: new[] { new BoardPauseCustomButton { id = "custom", text = "Custom" } },
            audioTracks: new[] { new BoardPauseAudioTrack { id = "music", name = "Music", value = 0.5f } });

        Assert.That(model.IsProfileSwitcherVisible, Is.True);
        Assert.That(model.CurrentPauseScreenContext.applicationName, Is.EqualTo("Naval Warfare"));
        Assert.That(model.CurrentPauseScreenContext.showSaveOptionUponExit, Is.True);
        Assert.That(model.CurrentPauseScreenContext.customButtons, Has.Length.EqualTo(1));
        Assert.That(model.CurrentPauseScreenContext.audioTracks, Has.Length.EqualTo(1));

        model.HideProfileSwitcher();
        model.ClearPauseScreenContext();

        Assert.That(model.IsProfileSwitcherVisible, Is.False);
        Assert.That(model.CurrentPauseScreenContext.applicationName, Is.EqualTo(UnityEngine.Application.productName));
        Assert.That(model.CurrentPauseScreenContext.customButtons, Is.Empty);
    }

    [Test]
    public void SessionMutations_PreserveRosterIndices_AndResetSeedsActiveProfile()
    {
        EmulatorModel model = CreateModel(CreateThreeProfileSessionData());

        bool replaced = model.ReplaceSessionPlayer(model.Players[1].sessionId, "profile-jules");

        Assert.That(replaced, Is.True);
        Assert.That(model.Players[1].playerId, Is.EqualTo("profile-jules"));

        bool removed = model.RemoveSessionPlayer(model.Players[0].sessionId);
        Assert.That(removed, Is.True);
        Assert.That(model.Players.Select(player => player.playerId), Is.EqualTo(new[] { "profile-jules" }));
        Assert.That(model.ActiveProfile.playerId, Is.EqualTo("profile-avery"));

        bool reset = model.ResetPlayers();
        Assert.That(reset, Is.True);
        Assert.That(model.Players, Has.Length.EqualTo(1));
        Assert.That(model.ActiveProfile.playerId, Is.EqualTo(model.Players[0].playerId));
    }

    [Test]
    public async Task SaveGameLifecycle_PersistsPayloadMetadataAndSessionRestore()
    {
        EmulatorModel model = CreateModel();
        model.AddSessionPlayer("profile-noor", BoardPlayerType.Profile);

        byte[] payload = { 1, 2, 3, 4 };
        BoardSaveGameMetadata created = await model.CreateSaveGame(payload, new BoardSaveGameMetadataChange
        {
            description = "Snapshot",
            playedTime = 42,
            gameVersion = "1.0.0"
        });

        Assert.That(created.id, Is.Not.Null.And.Not.Empty);
        Assert.That((await model.GetSaveGamesMetadata()), Has.Length.EqualTo(1));

        BoardSaveGameMetadata updated = await model.UpdateSaveGame(created.id, new byte[] { 9, 8, 7 }, new BoardSaveGameMetadataChange
        {
            description = "Updated Snapshot",
            playedTime = 128,
            gameVersion = "1.1.0"
        });

        Assert.That(updated.description, Is.EqualTo("Updated Snapshot"));
        Assert.That(updated.playedTime, Is.EqualTo(128));

        byte[] loadedPayload = await model.LoadSaveGame(created.id);
        Assert.That(loadedPayload, Is.EqualTo(new byte[] { 9, 8, 7 }));
        Assert.That(model.Players.Length, Is.GreaterThanOrEqualTo(1));

        bool removedActiveProfile = await model.RemoveActiveProfileFromSaveGame(created.id);
        Assert.That(removedActiveProfile, Is.True);
    }

    [Test]
    public void BlankProfileSeedData_IsNormalizedToValidBoardPlayerState()
    {
        EmulatorModel model = CreateModel(new EmulatorMockData
        {
            Profiles = new()
            {
                new EmulatorProfileData
                {
                    PlayerId = string.Empty,
                    DisplayName = null,
                    AvatarId = string.Empty
                }
            },
            Session = new EmulatorSessionData()
        });

        Assert.That(model.ActiveProfile.playerId, Is.Not.Null.And.Not.Empty);
        Assert.That(model.ActiveProfile.name, Is.EqualTo("Player 1"));
        Assert.That(model.ActiveProfile.avatarId, Is.EqualTo(model.ActiveProfile.playerId));
        Assert.That(model.Players, Has.Length.EqualTo(1));
        Assert.That(model.Players[0].name, Is.EqualTo("Player 1"));
    }

    [Test]
    public void SetActiveProfile_UpdatesActiveProfileWithoutReorderingSessionPlayers()
    {
        EmulatorModel model = CreateModel(CreateThreeProfileSessionData());
        int playersChangedCount = 0;
        int activeProfileChangedCount = 0;

        model.PlayersChanged += (_, _) => playersChangedCount++;
        model.ActiveProfileChanged += (_, _) => activeProfileChangedCount++;

        bool changed = model.SetActiveProfile("profile-noor");

        Assert.That(changed, Is.True);
        Assert.That(model.ActiveProfile.playerId, Is.EqualTo("profile-noor"));
        Assert.That(model.Players.Select(player => player.playerId), Is.EqualTo(new[] { "profile-avery", "profile-noor" }));
        Assert.That(playersChangedCount, Is.EqualTo(0));
        Assert.That(activeProfileChangedCount, Is.EqualTo(1));
    }

    [Test]
    public void SessionNormalization_AllowsActiveProfileOutsideSession()
    {
        EmulatorModel model = CreateModel(new EmulatorMockData
        {
            Profiles = new()
            {
                new EmulatorProfileData
                {
                    PlayerId = "profile-001",
                    DisplayName = "Player 1",
                    AvatarId = "0"
                },
                new EmulatorProfileData
                {
                    PlayerId = "profile-004",
                    DisplayName = "Player 4",
                    AvatarId = "1"
                }
            },
            Session = new EmulatorSessionData
            {
                ActiveProfileId = "profile-004",
                Players = new()
                {
                    new EmulatorSessionPlayerData
                    {
                        SessionId = 1,
                        PlayerId = "profile-001",
                        Type = BoardPlayerType.Profile
                    }
                }
            }
        });

        Assert.That(model.ActiveProfile.playerId, Is.EqualTo("profile-004"));
        Assert.That(model.Players[0].playerId, Is.EqualTo("profile-001"));
        Assert.That(model.Players, Has.Length.EqualTo(1));
    }

    [Test]
    public void SetActiveProfile_DoesNotRemoveExistingSessionEntryForSelectedProfile()
    {
        EmulatorModel model = CreateModel(new EmulatorMockData
        {
            Profiles = new()
            {
                new EmulatorProfileData
                {
                    PlayerId = "profile-avery",
                    DisplayName = "Commander Avery",
                    AvatarId = "0"
                },
                new EmulatorProfileData
                {
                    PlayerId = "profile-noor",
                    DisplayName = "Captain Noor",
                    AvatarId = "1"
                }
            },
            Session = new EmulatorSessionData
            {
                ActiveProfileId = "profile-avery",
                Players = new()
                {
                    new EmulatorSessionPlayerData
                    {
                        SessionId = 1,
                        PlayerId = "profile-avery",
                        Type = BoardPlayerType.Profile
                    },
                    new EmulatorSessionPlayerData
                    {
                        SessionId = 2,
                        PlayerId = "profile-noor",
                        Type = BoardPlayerType.Profile
                    }
                }
            }
        });

        bool changed = model.SetActiveProfile("profile-noor");

        Assert.That(changed, Is.True);
        Assert.That(model.Players.Select(player => player.playerId), Is.EqualTo(new[] { "profile-avery", "profile-noor" }));
    }

    [Test]
    public void CreateProfile_PersistsValidatedProfileWithAvatarBackgroundColor()
    {
        EmulatorModel model = CreateModel();
        int originalProfileCount = model.CurrentData.Profiles.Count;
        UnityEngine.Color avatarBackgroundColor = new(0.42f, 0.72f, 0.29f, 1f);

        bool created = model.CreateProfile("billybob", avatarBackgroundColor);

        Assert.That(created, Is.True);
        Assert.That(model.CurrentData.Profiles, Has.Count.EqualTo(originalProfileCount + 1));

        EmulatorProfileData createdProfile = model.CurrentData.Profiles.Last();
        Assert.That(createdProfile.DisplayName, Is.EqualTo("billybob"));
        Assert.That(createdProfile.Type, Is.EqualTo(BoardPlayerType.Profile));
        Assert.That(createdProfile.AvatarBackgroundColor, Is.EqualTo(avatarBackgroundColor));
    }

    [Test]
    public void CreateProfile_RejectsInvalidProfileNames()
    {
        EmulatorModel model = CreateModel();
        int originalProfileCount = model.CurrentData.Profiles.Count;

        bool created = model.CreateProfile("fuyyfytd!$", UnityEngine.Color.magenta);

        Assert.That(created, Is.False);
        Assert.That(model.CurrentData.Profiles, Has.Count.EqualTo(originalProfileCount));
    }

    [Test]
    public void CreateProfile_RejectsDuplicateProfileNames()
    {
        EmulatorModel model = CreateModel();
        int originalProfileCount = model.CurrentData.Profiles.Count;

        bool created = model.CreateProfile("Commander Avery", UnityEngine.Color.magenta);

        Assert.That(created, Is.False);
        Assert.That(model.CurrentData.Profiles, Has.Count.EqualTo(originalProfileCount));
    }

    [Test]
    public void UpdateProfile_PersistsEditedNameAndAvatarBackgroundColor()
    {
        EmulatorModel model = CreateModel();
        UnityEngine.Color avatarBackgroundColor = new(0.24f, 0.63f, 0.51f, 1f);

        bool updated = model.UpdateProfile("profile-avery", "AveryEdited", avatarBackgroundColor);

        Assert.That(updated, Is.True);

        EmulatorProfileData updatedProfile = model.CurrentData.Profiles.Single(profile => profile.PlayerId == "profile-avery");
        Assert.That(updatedProfile.DisplayName, Is.EqualTo("AveryEdited"));
        Assert.That(updatedProfile.AvatarBackgroundColor, Is.EqualTo(avatarBackgroundColor));
    }

    [Test]
    public void UpdateProfile_PreservesAuthoredAvatarWhenChangingBackgroundColor()
    {
        Texture2D authoredAvatar = new(4, 4);
        EmulatorModel model = CreateModel(new EmulatorMockData
        {
            Profiles = new()
            {
                new EmulatorProfileData
                {
                    PlayerId = "profile-001",
                    DisplayName = "Player1",
                    AvatarId = "0",
                    Avatar = authoredAvatar
                }
            },
            Session = new EmulatorSessionData
            {
                ActiveProfileId = "profile-001"
            }
        });

        try
        {
            bool updated = model.UpdateProfile("profile-001", "Player1", new UnityEngine.Color(0.2f, 0.4f, 0.6f, 1f));

            Assert.That(updated, Is.True);
            Assert.That(model.CurrentData.Profiles.Single().Avatar, Is.SameAs(authoredAvatar));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(authoredAvatar);
        }
    }

    [Test]
    public void UpdateProfile_RejectsDuplicateProfileNames()
    {
        EmulatorModel model = CreateModel();
        string originalName = model.CurrentData.Profiles.Single(profile => profile.PlayerId == "profile-avery").DisplayName;

        bool updated = model.UpdateProfile("profile-avery", "Captain Noor", UnityEngine.Color.cyan);

        Assert.That(updated, Is.False);
        Assert.That(model.CurrentData.Profiles.Single(profile => profile.PlayerId == "profile-avery").DisplayName, Is.EqualTo(originalName));
    }

    [Test]
    public void DeleteProfile_RemovesProfileAndFallsBackActiveProfileToFirstRemaining()
    {
        EmulatorModel model = CreateModel();
        model.SetActiveProfile("profile-noor");

        bool deleted = model.DeleteProfile("profile-noor");

        Assert.That(deleted, Is.True);
        Assert.That(model.CurrentData.Profiles.Any(profile => profile.PlayerId == "profile-noor"), Is.False);
        Assert.That(model.ActiveProfile.playerId, Is.EqualTo("profile-avery"));
        Assert.That(model.Players.Select(player => player.playerId), Does.Not.Contain("profile-noor"));
    }

    [Test]
    public async Task DeleteProfile_RemovesProfileFromSaveSnapshotsAndDeletesEmptySaves()
    {
        EmulatorModel model = CreateModel();
        model.AddSessionPlayer("profile-noor", BoardPlayerType.Profile);

        BoardSaveGameMetadata created = await model.CreateSaveGame(new byte[] { 1, 2 }, new BoardSaveGameMetadataChange
        {
            description = "Snapshot",
            playedTime = 10,
            gameVersion = "1.0.0"
        });

        bool deleted = model.DeleteProfile("profile-noor");

        Assert.That(deleted, Is.True);
        BoardSaveGameMetadata[] metadata = await model.GetSaveGamesMetadata();
        Assert.That(metadata, Has.Length.EqualTo(1));
        Assert.That(model.CurrentData.SaveGames.Single(save => save.SaveId == created.id).Players.Select(player => player.PlayerId), Does.Not.Contain("profile-noor"));
    }

    [Test]
    public void StaleSessionProfileReference_IsNormalizedWithoutCreatingGuestProfile()
    {
        EmulatorMockData mockData = EmulatorDefaults.CreateMockData();
        mockData.Session.Players.Add(new EmulatorSessionPlayerData
        {
            SessionId = 2,
            PlayerId = "deleted-profile"
        });

        EmulatorModel model = CreateModel(mockData);

        Assert.That(model.CurrentData.Profiles.Any(profile => profile.DisplayName == "Player 1"), Is.False);
        Assert.That(model.Players.Select(player => player.playerId), Does.Not.Contain("deleted-profile"));
    }

    [Test]
    public void AddSessionPlayer_Guest_PersistsOnlyInSessionState()
    {
        EmulatorModel model = CreateModel();
        int originalProfileCount = model.CurrentData.Profiles.Count;

        bool added = model.AddSessionPlayer(playerId: null, playerType: BoardPlayerType.Guest);

        Assert.That(added, Is.True);
        Assert.That(model.CurrentData.Profiles, Has.Count.EqualTo(originalProfileCount));
        Assert.That(model.Players, Has.Length.EqualTo(2));
        Assert.That(model.Players.Last().type, Is.EqualTo(BoardPlayerType.Guest));
        Assert.That(model.CurrentData.Session.Players.Last().Type, Is.EqualTo(BoardPlayerType.Guest));
        Assert.That(model.CurrentData.Session.Players.Last().DisplayName, Does.StartWith("Player"));
    }

    [Test]
    public void DuplicateGuestDisplayNames_AreNormalizedToUniqueValues()
    {
        EmulatorMockData mockData = EmulatorDefaults.CreateMockData();
        mockData.Session.Players.Add(new EmulatorSessionPlayerData
        {
            SessionId = 2,
            PlayerId = "guest-001",
            Type = BoardPlayerType.Guest,
            DisplayName = "Commander Avery"
        });
        mockData.Session.Players.Add(new EmulatorSessionPlayerData
        {
            SessionId = 3,
            PlayerId = "guest-002",
            Type = BoardPlayerType.Guest,
            DisplayName = "Commander Avery"
        });

        EmulatorModel model = CreateModel(mockData);
        EmulatorSessionPlayerData[] guestPlayers = model.CurrentData.Session.Players
            .Where(player => player.Type == BoardPlayerType.Guest)
            .ToArray();

        Assert.That(guestPlayers, Has.Length.EqualTo(2));
        Assert.That(guestPlayers.Select(player => player.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase), Has.Count.EqualTo(2));
        Assert.That(guestPlayers.Any(player => string.Equals(player.DisplayName, "Commander Avery", StringComparison.OrdinalIgnoreCase)), Is.False);
    }

    [Test]
    public void AddSessionPlayer_Guest_UsesUniqueDisplayNameAgainstExistingGuests()
    {
        EmulatorMockData mockData = EmulatorDefaults.CreateMockData();
        mockData.Session.Players.Add(new EmulatorSessionPlayerData
        {
            SessionId = 2,
            PlayerId = "guest-001",
            Type = BoardPlayerType.Guest,
            DisplayName = "Player 1"
        });
        mockData.Session.Players.Add(new EmulatorSessionPlayerData
        {
            SessionId = 3,
            PlayerId = "guest-002",
            Type = BoardPlayerType.Guest,
            DisplayName = "Player 3"
        });

        EmulatorModel model = CreateModel(mockData);

        bool added = model.AddSessionPlayer(playerId: null, playerType: BoardPlayerType.Guest);

        Assert.That(added, Is.True);
        EmulatorSessionPlayerData addedGuest = model.CurrentData.Session.Players.Last();
        Assert.That(addedGuest.Type, Is.EqualTo(BoardPlayerType.Guest));
        Assert.That(addedGuest.DisplayName, Is.EqualTo("Player 2"));
    }

    [Test]
    public void ActiveMockDataAssetChange_RebindsModelToNewAssetInstance()
    {
        EmulatorMockDataAsset firstAsset = UnityEngine.ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
        EmulatorMockDataAsset secondAsset = UnityEngine.ScriptableObject.CreateInstance<EmulatorMockDataAsset>();
        firstAsset.Replace(new EmulatorMockData
        {
            Profiles = new()
            {
                new EmulatorProfileData
                {
                    PlayerId = "profile-001",
                    DisplayName = "First",
                    AvatarId = "0"
                }
            },
            Session = new EmulatorSessionData
            {
                ActiveProfileId = "profile-001"
            }
        });
        secondAsset.Replace(new EmulatorMockData
        {
            Profiles = new()
            {
                new EmulatorProfileData
                {
                    PlayerId = "profile-001",
                    DisplayName = "Second",
                    AvatarId = "0"
                }
            },
            Session = new EmulatorSessionData
            {
                ActiveProfileId = "profile-001"
            }
        });

        try
        {
            EmulatorProjectSettingsBridge.SetActiveMockDataAsset(firstAsset);
            using EmulatorModel model = CreateModel();
            int changedCount = 0;
            model.Changed += (_, _) => changedCount++;

            EmulatorProjectSettingsBridge.SetActiveMockDataAsset(secondAsset);

            Assert.That(model.CurrentData.Profiles.Single().DisplayName, Is.EqualTo("Second"));
            Assert.That(changedCount, Is.EqualTo(1));
        }
        finally
        {
            EmulatorProjectSettingsBridge.SetActiveMockDataAsset(null);
            UnityEngine.Object.DestroyImmediate(firstAsset);
            UnityEngine.Object.DestroyImmediate(secondAsset);
        }
    }

    [Test]
    public void RemoveSessionPlayer_RejectsRemovingLastNonGuestPlayer()
    {
        EmulatorModel model = CreateModel();

        bool removed = model.RemoveSessionPlayer(model.Players[0].sessionId);

        Assert.That(removed, Is.False);
        Assert.That(model.Players, Has.Length.EqualTo(1));
        Assert.That(model.Players[0].playerId, Is.EqualTo("profile-avery"));
    }

    [Test]
    public void GuestOnlySession_IsNormalizedToIncludeANonGuestPlayer()
    {
        EmulatorModel model = CreateModel(new EmulatorMockData
        {
            Profiles = new()
            {
                new EmulatorProfileData
                {
                    PlayerId = "profile-001",
                    DisplayName = "Player 1",
                    AvatarId = "0",
                    Type = BoardPlayerType.Profile
                }
            },
            Session = new EmulatorSessionData
            {
                ActiveProfileId = "profile-001",
                Players = new()
                {
                    new EmulatorSessionPlayerData
                    {
                        SessionId = 1,
                        PlayerId = "guest-001",
                        Type = BoardPlayerType.Guest,
                        DisplayName = "Player 1"
                    }
                }
            }
        });

        Assert.That(model.Players.Count(player => player.type != BoardPlayerType.Guest), Is.EqualTo(1));
        Assert.That(model.Players.Select(player => player.playerId), Is.EqualTo(new[] { "guest-001", "profile-001" }));
    }

    private static EmulatorModel CreateModel(EmulatorMockData mockData = null)
    {
        return new EmulatorModel(
            new TestLoggerFactory(),
            new BoardSdkObjectFactory(),
            mockData ?? EmulatorDefaults.CreateMockData());
    }

    private static EmulatorMockData CreateThreeProfileSessionData()
    {
        EmulatorMockData mockData = EmulatorDefaults.CreateMockData();
        mockData.Profiles.Add(new EmulatorProfileData
        {
            PlayerId = "profile-jules",
            DisplayName = "Pilot Jules",
            AvatarId = "2",
            Type = BoardPlayerType.Profile,
            AvatarBackgroundColor = new UnityEngine.Color(0.3f, 0.72f, 0.52f, 1f)
        });
        mockData.Session.Players.Add(new EmulatorSessionPlayerData
        {
            SessionId = 2,
            PlayerId = "profile-noor",
            Type = BoardPlayerType.Profile
        });
        return mockData;
    }

    private sealed class TestLoggerFactory : ILoggerFactory
    {
        public IRahmenLogger Get<T>(object context) where T : ILogChannel
        {
            return new NullLogger();
        }

        public IRahmenLogger Get(Type channelType, object context)
        {
            return new NullLogger();
        }
    }

    private sealed class NullLogger : IRahmenLogger
    {
        public ICanLog Trace(LogOptions options = LogOptions.None) => null;
        public ICanLog Debug(LogOptions options = LogOptions.None) => null;
        public ICanLog Info(LogOptions options = LogOptions.None) => null;
        public ICanLog Warning(LogOptions options = LogOptions.None) => null;
        public ICanLog Error(LogOptions options = LogOptions.None) => null;
        public ICanLog Fatal(LogOptions options = LogOptions.None) => null;
        public ICanLog At(LogLevels level, LogOptions options = LogOptions.None) => null;
    }
}
}
