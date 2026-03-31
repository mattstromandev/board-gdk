using System;
using System.Threading.Tasks;

using Board.Core;
using Board.Save;

using BoardGDK.BoardAdapters;

using BE.Emulator.Data;
using BE.Emulator.Services;
using BE.Emulator.Utility;

using NUnit.Framework;

using UnityEngine;

using EmulatorBoardApplication = BE.Emulator.Core.BoardApplication;
using EmulatorBoardSession = BE.Emulator.Session.BoardSession;
using EmulatorBoardSaveGameManager = BE.Emulator.Save.BoardSaveGameManager;

namespace BE.Emulator.Tests
{
public sealed class BoardStaticApiTests
{
    [TearDown]
    public void TearDown()
    {
        BoardStaticApiRegistry.Clear();
    }

    [Test]
    public void StaticFacade_ThrowsNotReady_WhenAccessedBeforeInitialization()
    {
        Assert.That(() => _ = EmulatorBoardSession.players, Throws.TypeOf<EmulatorNotReadyException>());
        Assert.That(() => EmulatorBoardApplication.ShowProfileSwitcher(), Throws.TypeOf<EmulatorNotReadyException>());
        Assert.That(() => EmulatorBoardSaveGameManager.GetMaxPayloadSize(), Throws.TypeOf<EmulatorNotReadyException>());
    }

    [Test]
    public async Task StaticFacade_DelegatesToInjectedBoardInterfaces()
    {
        TestBoardApplication application = new();
        TestBoardSession session = new();
        TestBoardSaveGameManager saveGameManager = new();
        BoardStaticApiBinder binder = new(application, session, saveGameManager);

        binder.Initialize();

        bool customButtonRaised = false;
        bool pauseActionRaised = false;
        bool playersChangedRaised = false;
        bool activeProfileChangedRaised = false;

        global::Board.Core.PauseScreenCustomButtonPressedHandler customHandler = (_, _) => customButtonRaised = true;
        global::Board.Core.PauseScreenActionReceivedHandler pauseHandler = (_, _) => pauseActionRaised = true;
        Action playersChangedHandler = () => playersChangedRaised = true;
        Action activeProfileChangedHandler = () => activeProfileChangedRaised = true;

        EmulatorBoardApplication.customPauseScreenButtonPressed += customHandler;
        EmulatorBoardApplication.pauseScreenActionReceived += pauseHandler;
        EmulatorBoardSession.playersChanged += playersChangedHandler;
        EmulatorBoardSession.activeProfileChanged += activeProfileChangedHandler;

        EmulatorBoardApplication.ShowProfileSwitcher();
        EmulatorBoardApplication.HideProfileSwitcher();
        EmulatorBoardApplication.SetPauseScreenContext(new BoardPauseScreenContext { applicationName = "Naval Warfare" });
        EmulatorBoardApplication.UpdatePauseScreenContext(applicationName: "Updated");
        EmulatorBoardApplication.ClearPauseScreenContext();
        EmulatorBoardApplication.Exit();

        Assert.That(application.ShowProfileSwitcherCalls, Is.EqualTo(1));
        Assert.That(application.HideProfileSwitcherCalls, Is.EqualTo(1));
        Assert.That(application.SetPauseScreenContextCalls, Is.EqualTo(1));
        Assert.That(application.UpdatePauseScreenContextCalls, Is.EqualTo(1));
        Assert.That(application.ClearPauseScreenContextCalls, Is.EqualTo(1));
        Assert.That(application.ExitCalls, Is.EqualTo(1));

        application.RaiseCustomPauseScreenButtonPressed("settings", Array.Empty<BoardPauseAudioTrack>());
        application.RaisePauseScreenActionReceived(BoardPauseAction.Resume, Array.Empty<BoardPauseAudioTrack>());
        session.RaisePlayersChanged();
        session.RaiseActiveProfileChanged();

        Assert.That(customButtonRaised, Is.True);
        Assert.That(pauseActionRaised, Is.True);
        Assert.That(playersChangedRaised, Is.True);
        Assert.That(activeProfileChangedRaised, Is.True);
        Assert.That(EmulatorBoardSession.players, Is.SameAs(session.Players));
        Assert.That(EmulatorBoardSession.activeProfile, Is.SameAs(session.ActiveProfile));
        Assert.That(await EmulatorBoardSession.PresentAddPlayerSelector(), Is.True);
        Assert.That(EmulatorBoardSession.ResetPlayers(), Is.True);
        Assert.That(await EmulatorBoardSession.PresentReplacePlayerSelector(session.Players[0]), Is.True);
        Assert.That(await EmulatorBoardSaveGameManager.GetAppStorageInfo(), Is.EqualTo(saveGameManager.AppStorageInfo));
        Assert.That(EmulatorBoardSaveGameManager.GetMaxPayloadSize(), Is.EqualTo(saveGameManager.MaxPayloadSize));
        Assert.That(EmulatorBoardSaveGameManager.GetMaxAppStorage(), Is.EqualTo(saveGameManager.MaxAppStorage));
        Assert.That(EmulatorBoardSaveGameManager.GetMaxSaveDescriptionLength(), Is.EqualTo(saveGameManager.MaxSaveDescriptionLength));
        Assert.That(await EmulatorBoardSaveGameManager.GetSaveGamesMetadata(), Is.SameAs(saveGameManager.Metadata));
        Assert.That(await EmulatorBoardSaveGameManager.CreateSaveGame(Array.Empty<byte>(), new BoardSaveGameMetadataChange()), Is.SameAs(saveGameManager.CreatedMetadata));
        Assert.That(await EmulatorBoardSaveGameManager.UpdateSaveGame("save-1", Array.Empty<byte>(), new BoardSaveGameMetadataChange()), Is.SameAs(saveGameManager.UpdatedMetadata));
        Assert.That(await EmulatorBoardSaveGameManager.LoadSaveGame("save-1"), Is.SameAs(saveGameManager.Payload));
        Assert.That(await EmulatorBoardSaveGameManager.LoadSaveGameCoverImage("save-1"), Is.SameAs(saveGameManager.CoverImage));
        Assert.That(await EmulatorBoardSaveGameManager.RemovePlayersFromSaveGame("save-1"), Is.True);
        Assert.That(await EmulatorBoardSaveGameManager.RemoveActiveProfileFromSaveGame("save-1"), Is.True);

        EmulatorBoardApplication.customPauseScreenButtonPressed -= customHandler;
        EmulatorBoardApplication.pauseScreenActionReceived -= pauseHandler;
        EmulatorBoardSession.playersChanged -= playersChangedHandler;
        EmulatorBoardSession.activeProfileChanged -= activeProfileChangedHandler;

        binder.Dispose();

        Assert.That(() => _ = EmulatorBoardSession.players, Throws.TypeOf<EmulatorNotReadyException>());
    }

    private sealed class TestBoardApplication : IBoardApplication
    {
        public bool IsProfileSwitcherVisible => false;
        public BoardPauseScreenContext CurrentPauseScreenContext => new();

        public event global::Board.Core.PauseScreenCustomButtonPressedHandler CustomPauseScreenButtonPressed;
        public event global::Board.Core.PauseScreenActionReceivedHandler PauseScreenActionReceived;

        public int ShowProfileSwitcherCalls { get; private set; }
        public int HideProfileSwitcherCalls { get; private set; }
        public int SetPauseScreenContextCalls { get; private set; }
        public int UpdatePauseScreenContextCalls { get; private set; }
        public int ClearPauseScreenContextCalls { get; private set; }
        public int ExitCalls { get; private set; }

        public void ShowProfileSwitcher() => ShowProfileSwitcherCalls++;
        public void HideProfileSwitcher() => HideProfileSwitcherCalls++;
        public void SetPauseScreenContext(BoardPauseScreenContext context) => SetPauseScreenContextCalls++;
        public void SetPauseScreenContext(string applicationName = null, bool? showSaveOptionUponExit = null, BoardPauseCustomButton[] customButtons = null, BoardPauseAudioTrack[] audioTracks = null) => SetPauseScreenContextCalls++;
        public void UpdatePauseScreenContext(string applicationName = null, bool? showSaveOptionUponExit = null, BoardPauseCustomButton[] customButtons = null, BoardPauseAudioTrack[] audioTracks = null) => UpdatePauseScreenContextCalls++;
        public void ClearPauseScreenContext() => ClearPauseScreenContextCalls++;
        public void Exit() => ExitCalls++;

        public void RaiseCustomPauseScreenButtonPressed(string customButtonId, BoardPauseAudioTrack[] audioTracks) => CustomPauseScreenButtonPressed?.Invoke(customButtonId, audioTracks);
        public void RaisePauseScreenActionReceived(BoardPauseAction pauseAction, BoardPauseAudioTrack[] audioTracks) => PauseScreenActionReceived?.Invoke(pauseAction, audioTracks);
    }

    private sealed class TestBoardSession : IBoardSession
    {
        public TestBoardSession()
        {
            BoardSdkObjectFactory factory = new();
            EmulatorProfileData profile = new()
            {
                PlayerId = "player-1",
                DisplayName = "Player 1",
                AvatarId = "1",
                Type = BoardPlayerType.Profile
            };
            EmulatorSessionPlayerData sessionPlayer = new()
            {
                SessionId = 1,
                PlayerId = profile.PlayerId
            };

            Players = new[]
            {
                factory.CreateSessionPlayer(sessionPlayer, profile)
            };

            ActiveProfile = factory.CreateBoardPlayer(profile);
        }

        public global::Board.Session.BoardSessionPlayer[] Players { get; }
        public BoardPlayer ActiveProfile { get; }

        public event Action PlayersChanged;
        public event Action ActiveProfileChanged;

        public Task<bool> PresentAddPlayerSelector() => Task.FromResult(true);
        public bool ResetPlayers() => true;
        public Task<bool> PresentReplacePlayerSelector(global::Board.Session.BoardSessionPlayer player) => Task.FromResult(true);

        public void RaisePlayersChanged() => PlayersChanged?.Invoke();
        public void RaiseActiveProfileChanged() => ActiveProfileChanged?.Invoke();
    }

    private sealed class TestBoardSaveGameManager : IBoardSaveGameManager
    {
        public TestBoardSaveGameManager()
        {
            AppStorageInfo = new BoardAppStorageInfo
            {
                totalStorage = 100,
                usedStorage = 25,
                remainingStorage = 75,
                usagePercentage = 0.25f
            };

            Metadata = new[]
            {
                new BoardSaveGameMetadata
                {
                    id = "save-1",
                    description = "Test Save"
                }
            };

            CreatedMetadata = new BoardSaveGameMetadata
            {
                id = "created-save"
            };

            UpdatedMetadata = new BoardSaveGameMetadata
            {
                id = "updated-save"
            };

            Payload = new byte[] { 1, 2, 3 };
            CoverImage = new Texture2D(2, 2);
        }

        public BoardAppStorageInfo AppStorageInfo { get; }
        public long MaxPayloadSize => 128;
        public long MaxAppStorage => 1024;
        public int MaxSaveDescriptionLength => 100;
        public BoardSaveGameMetadata[] Metadata { get; }
        public BoardSaveGameMetadata CreatedMetadata { get; }
        public BoardSaveGameMetadata UpdatedMetadata { get; }
        public byte[] Payload { get; }
        public Texture2D CoverImage { get; }

        public Task<BoardAppStorageInfo> GetAppStorageInfo() => Task.FromResult(AppStorageInfo);
        public long GetMaxPayloadSize() => MaxPayloadSize;
        public long GetMaxAppStorage() => MaxAppStorage;
        public int GetMaxSaveDescriptionLength() => MaxSaveDescriptionLength;
        public Task<BoardSaveGameMetadata[]> GetSaveGamesMetadata() => Task.FromResult(Metadata);
        public Task<BoardSaveGameMetadata> CreateSaveGame(byte[] payload, BoardSaveGameMetadataChange metadataChange) => Task.FromResult(CreatedMetadata);
        public Task<BoardSaveGameMetadata> UpdateSaveGame(string saveId, byte[] payload, BoardSaveGameMetadataChange metadataChange) => Task.FromResult(UpdatedMetadata);
        public Task<byte[]> LoadSaveGame(string saveId) => Task.FromResult(Payload);
        public Task<Texture2D> LoadSaveGameCoverImage(string saveId) => Task.FromResult(CoverImage);
        public Task<bool> RemovePlayersFromSaveGame(string saveId) => Task.FromResult(true);
        public Task<bool> RemoveActiveProfileFromSaveGame(string saveId) => Task.FromResult(true);
    }
}
}
