using System;
using System.Threading.Tasks;

using Board.Core;

using BE.Emulator.Actions;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;
using BE.Emulator.Services;
using BE.Emulator.Utility;

using NUnit.Framework;

using Rahmen.Logging;

namespace BE.Emulator.Tests
{
public sealed class BoardSessionServiceTests
{
    [SetUp]
    public void SetUp()
    {
        EmulatorProjectSettingsBridge.SetActiveMockDataAsset(null);
    }

    [Test]
    public async Task PresentAddPlayerSelector_PublishesViewAction_AndCompletesFalseOnDismiss()
    {
        BoardSessionService service = CreateService();
        IViewAction requestedAction = null;

        void OnViewActionRequested(object sender, ViewActionEventArgs eventArgs)
        {
            requestedAction = eventArgs?.Action;
        }

        EmulatorExternalViewActionBridge.ViewActionRequested += OnViewActionRequested;
        try
        {
            Task<bool> resultTask = service.PresentAddPlayerSelector();

            Assert.That(requestedAction, Is.TypeOf<PresentAddPlayerSelectorViewAction>());

            service.HandleRoutedViewAction(this, new CloseAddPlayerSelectorViewAction());

            Assert.That(await resultTask, Is.False);
        }
        finally
        {
            EmulatorExternalViewActionBridge.ViewActionRequested -= OnViewActionRequested;
            service.Dispose();
        }
    }

    [Test]
    public async Task PresentAddPlayerSelector_AddsSelectedPlayer_AndCompletesTrue()
    {
        EmulatorModel model = CreateModel();
        BoardSessionService service = new(model);

        try
        {
            Task<bool> resultTask = service.PresentAddPlayerSelector();

            service.HandleRoutedViewAction(this, new PlayerAddedViewAction
            {
                SelectedProfileId = "profile-noor",
                PlayerType = BoardPlayerType.Profile
            });

            Assert.That(await resultTask, Is.True);
            Assert.That(model.Players, Has.Length.EqualTo(2));
            Assert.That(model.Players[1].playerId, Is.EqualTo("profile-noor"));
        }
        finally
        {
            service.Dispose();
        }
    }

    [Test]
    public async Task PresentReplacePlayerSelector_PublishesViewAction_AndCompletesFalseOnDismiss()
    {
        EmulatorModel model = CreateThreeProfileModel();
        BoardSessionService service = new(model);
        IViewAction requestedAction = null;

        void OnViewActionRequested(object sender, ViewActionEventArgs eventArgs)
        {
            requestedAction = eventArgs?.Action;
        }

        EmulatorExternalViewActionBridge.ViewActionRequested += OnViewActionRequested;
        try
        {
            Task<bool> resultTask = service.PresentReplacePlayerSelector(model.Players[1]);

            Assert.That(requestedAction, Is.TypeOf<PresentReplacePlayerSelectorViewAction>());
            Assert.That(((PresentReplacePlayerSelectorViewAction)requestedAction).TargetSessionId, Is.EqualTo(model.Players[1].sessionId));

            service.HandleRoutedViewAction(this, new CloseReplacePlayerSelectorViewAction());

            Assert.That(await resultTask, Is.False);
        }
        finally
        {
            EmulatorExternalViewActionBridge.ViewActionRequested -= OnViewActionRequested;
            service.Dispose();
        }
    }

    [Test]
    public async Task PresentReplacePlayerSelector_ReplacesSelectedPlayer_AndCompletesTrue()
    {
        EmulatorModel model = CreateThreeProfileModel();
        BoardSessionService service = new(model);

        try
        {
            Task<bool> resultTask = service.PresentReplacePlayerSelector(model.Players[1]);

            service.HandleRoutedViewAction(this, new PlayerReplacedViewAction
            {
                TargetSessionId = model.Players[1].sessionId,
                SelectedProfileId = "profile-jules"
            });

            Assert.That(await resultTask, Is.True);
            Assert.That(model.Players[1].playerId, Is.EqualTo("profile-jules"));
        }
        finally
        {
            service.Dispose();
        }
    }

    [Test]
    public async Task PresentReplacePlayerSelector_RemovesSelectedPlayer_AndCompletesTrue()
    {
        EmulatorModel model = CreateModel();
        model.AddSessionPlayer(playerId: null, playerType: BoardPlayerType.Guest);
        BoardSessionService service = new(model);

        try
        {
            Task<bool> resultTask = service.PresentReplacePlayerSelector(model.Players[1]);

            service.HandleRoutedViewAction(this, new PlayerRemovedViewAction
            {
                TargetSessionId = model.Players[1].sessionId
            });

            Assert.That(await resultTask, Is.True);
            Assert.That(service.Players, Has.Length.EqualTo(1));
            Assert.That(service.Players[0].type, Is.EqualTo(BoardPlayerType.Profile));
        }
        finally
        {
            service.Dispose();
        }
    }

    [Test]
    public void PresentPlayerSelector_ThrowsWhenAnotherSelectorIsAlreadyPending()
    {
        EmulatorModel model = CreateThreeProfileModel();
        BoardSessionService service = new(model);

        try
        {
            _ = service.PresentAddPlayerSelector();

            Assert.That(() => service.PresentReplacePlayerSelector(model.Players[0]), Throws.TypeOf<InvalidOperationException>());
        }
        finally
        {
            service.Dispose();
        }
    }

    private static BoardSessionService CreateService()
    {
        return new BoardSessionService(CreateModel());
    }

    private static EmulatorModel CreateModel()
    {
        return new EmulatorModel(
            new TestLoggerFactory(),
            new BoardSdkObjectFactory(),
            EmulatorDefaults.CreateMockData());
    }

    private static EmulatorModel CreateThreeProfileModel()
    {
        var mockData = EmulatorDefaults.CreateMockData();
        mockData.Profiles.Add(new Emulator.Data.EmulatorProfileData
        {
            PlayerId = "profile-jules",
            DisplayName = "Pilot Jules",
            AvatarId = "2",
            Type = Board.Core.BoardPlayerType.Profile,
            AvatarBackgroundColor = new UnityEngine.Color(0.3f, 0.72f, 0.52f, 1f)
        });
        mockData.Session.Players.Add(new BE.Emulator.Data.EmulatorSessionPlayerData
        {
            SessionId = 2,
            PlayerId = "profile-noor",
            Type = Board.Core.BoardPlayerType.Profile
        });

        return new EmulatorModel(
            new TestLoggerFactory(),
            new BoardSdkObjectFactory(),
            mockData);
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
