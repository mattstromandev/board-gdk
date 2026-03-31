using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Board.Core;
using Board.Save;
using Board.Session;

using BE.Emulator.Data;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;
using BE.Emulator.Utility;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;

using Zenject;

namespace BE.Emulator
{
/// <summary>
/// Backing mock Board state for the emulator and Board wrapper services.
/// </summary>
internal sealed class EmulatorModel : IModel, IEmulatorModel, IDisposable
{
    private const int MinProfileNameLength = 2;
    private const int MaxProfileNameLength = 14;

    private readonly IRahmenLogger _logger;
    private readonly BoardSdkObjectFactory _objectFactory;
    private EmulatorMockDataAsset _asset;
    private EmulatorMockData _runtimeData;
    private bool _isWritingAsset;

    /// <summary>
    /// Creates the emulator model.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used for diagnostics.</param>
    /// <param name="objectFactory">The factory used to project emulator data into Board SDK objects.</param>
    /// <param name="codeOverride">Optional in-memory mock data override.</param>
    public EmulatorModel(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] BoardSdkObjectFactory objectFactory,
        [InjectOptional]
        [CanBeNull] EmulatorMockData codeOverride = null)
    {
        _logger = loggerFactory?.Get<LogChannels.BoardEmulation>(this) ?? throw new ArgumentNullException(nameof(loggerFactory));
        _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
        _asset = EmulatorProjectSettingsBridge.ActiveMockDataAsset;
        EmulatorProjectSettingsBridge.ActiveMockDataAssetChanged += OnActiveMockDataAssetChanged;

        if(_asset != null)
        {
            InitializeRuntimeDataFromAsset(codeOverride);
            _asset.Changed += OnAssetChanged;
        }
        else
        {
            _runtimeData = EmulatorMockDataUtility.Clone(codeOverride ?? EmulatorDefaults.CreateMockData());
            Normalize(CurrentData);
        }
    }

    /// <inheritdoc />
    public event EventHandler Changed;
    /// <inheritdoc />
    public event EventHandler PlayersChanged;
    /// <inheritdoc />
    public event EventHandler ActiveProfileChanged;
    /// <inheritdoc />
    public event EventHandler PauseScreenContextChanged;
    /// <inheritdoc />
    public event EventHandler SaveGamesChanged;

    /// <inheritdoc />
    public EmulatorMockData CurrentData
    {
        get
        {
            RefreshActiveAssetReference();

            if(_runtimeData == null)
            {
                _runtimeData = EmulatorMockDataUtility.Clone(EmulatorDefaults.CreateMockData());
                if(_asset != null)
                {
                    PersistRuntimeDataToAsset();
                }
            }

            Normalize(_runtimeData);
            return _runtimeData;
        }
    }

    /// <inheritdoc />
    public bool IsProfileSwitcherVisible => CurrentData.Application.IsProfileSwitcherVisible;
    /// <inheritdoc />
    public BoardPauseScreenContext CurrentPauseScreenContext => _objectFactory.CreatePauseScreenContext(CurrentData.Application, Application.productName);
    /// <inheritdoc />
    public BoardSessionPlayer[] Players => BuildPlayers();
    /// <inheritdoc />
    public BoardPlayer ActiveProfile => BuildActiveProfile();

    /// <inheritdoc />
    public void Dispose()
    {
        EmulatorProjectSettingsBridge.ActiveMockDataAssetChanged -= OnActiveMockDataAssetChanged;

        if(_asset != null)
        {
            _asset.Changed -= OnAssetChanged;
        }
    }

    /// <inheritdoc />
    public void ShowProfileSwitcher()
    {
        CurrentData.Application.IsProfileSwitcherVisible = true;
        PersistAndNotify(saveAsset: false);
    }

    /// <inheritdoc />
    public void HideProfileSwitcher()
    {
        CurrentData.Application.IsProfileSwitcherVisible = false;
        PersistAndNotify(saveAsset: false);
    }

    /// <inheritdoc />
    public void SetPauseScreenContext(BoardPauseScreenContext context)
    {
        SetPauseScreenContext(context.applicationName, context.showSaveOptionUponExit, context.customButtons, context.audioTracks);
    }

    /// <inheritdoc />
    public void SetPauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        BoardPauseCustomButton[] customButtons = null,
        BoardPauseAudioTrack[] audioTracks = null)
    {
        EmulatorApplicationData application = CurrentData.Application;
        application.HasPauseScreenContext = true;
        application.ApplicationName = applicationName ?? Application.productName;
        application.ShowSaveOptionUponExit = showSaveOptionUponExit ?? false;
        application.CustomButtons = customButtons?.ToList() ?? new List<BoardPauseCustomButton>();
        application.AudioTracks = audioTracks?.ToList() ?? new List<BoardPauseAudioTrack>();
        PersistAndNotify(saveAsset: false, raisePauseScreenContextChanged: true);
    }

    /// <inheritdoc />
    public void UpdatePauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        BoardPauseCustomButton[] customButtons = null,
        BoardPauseAudioTrack[] audioTracks = null)
    {
        EmulatorApplicationData application = CurrentData.Application;
        if(application.HasPauseScreenContext == false)
        {
            application.ApplicationName = Application.productName;
            application.ShowSaveOptionUponExit = false;
            application.CustomButtons = new List<BoardPauseCustomButton>();
            application.AudioTracks = new List<BoardPauseAudioTrack>();
        }

        application.HasPauseScreenContext = true;
        application.ApplicationName = applicationName ?? application.ApplicationName ?? Application.productName;
        application.ShowSaveOptionUponExit = showSaveOptionUponExit ?? application.ShowSaveOptionUponExit;
        application.CustomButtons = customButtons != null ? customButtons.ToList() : application.CustomButtons ?? new List<BoardPauseCustomButton>();
        application.AudioTracks = audioTracks != null ? audioTracks.ToList() : application.AudioTracks ?? new List<BoardPauseAudioTrack>();
        PersistAndNotify(saveAsset: false, raisePauseScreenContextChanged: true);
    }

    /// <inheritdoc />
    public void ClearPauseScreenContext()
    {
        EmulatorApplicationData application = CurrentData.Application;
        application.HasPauseScreenContext = false;
        application.ApplicationName = null;
        application.ShowSaveOptionUponExit = false;
        application.CustomButtons = new List<BoardPauseCustomButton>();
        application.AudioTracks = new List<BoardPauseAudioTrack>();
        PersistAndNotify(saveAsset: false, raisePauseScreenContextChanged: true);
    }

    public bool ResetPlayers()
    {
        EmulatorMockData data = CurrentData;
        EnsureActiveProfile(data);

        data.Session.Players.Clear();
        data.Session.Players.Add(new EmulatorSessionPlayerData
        {
            SessionId = 1,
            PlayerId = data.Session.ActiveProfileId
        });

        PersistAndNotify(raisePlayersChanged: true, raiseActiveProfileChanged: true);
        return true;
    }

    /// <inheritdoc />
    public bool AddSessionPlayer(string playerId, BoardPlayerType playerType)
    {
        EmulatorMockData data = CurrentData;

        switch(playerType)
        {
            case BoardPlayerType.Profile:
                if(string.IsNullOrWhiteSpace(playerId))
                {
                    return false;
                }

                if(data.Profiles.Any(profile => string.Equals(profile?.PlayerId, playerId, StringComparison.Ordinal)) == false)
                {
                    return false;
                }

                if(data.Session.Players.Any(player => string.Equals(player?.PlayerId, playerId, StringComparison.Ordinal)))
                {
                    return false;
                }

                data.Session.Players.Add(new EmulatorSessionPlayerData
                {
                    SessionId = AllocateSessionId(data),
                    PlayerId = playerId,
                    Type = BoardPlayerType.Profile
                });
                break;

            case BoardPlayerType.Guest:
                EmulatorSessionPlayerData guestPlayer = CreateGuestSessionPlayer(data);
                guestPlayer.SessionId = AllocateSessionId(data);
                data.Session.Players.Add(guestPlayer);
                break;

            default:
                return false;
        }

        PersistAndNotify(raisePlayersChanged: true);
        return true;
    }

    /// <inheritdoc />
    public bool RemoveSessionPlayer(int sessionId)
    {
        EmulatorMockData data = CurrentData;
        EmulatorSessionPlayerData existingPlayer = data.Session.Players.FirstOrDefault(sessionPlayer => sessionPlayer.SessionId == sessionId);
        if(existingPlayer == null)
        {
            return false;
        }

        if(existingPlayer.Type != BoardPlayerType.Guest
        && data.Session.Players.Count(sessionPlayer => sessionPlayer?.Type != BoardPlayerType.Guest) <= 1)
        {
            return false;
        }

        data.Session.Players.Remove(existingPlayer);
        PersistAndNotify(raisePlayersChanged: true);
        return true;
    }

    /// <inheritdoc />
    public bool ReplaceSessionPlayer(int targetSessionId, string replacementProfileId)
    {
        if(string.IsNullOrWhiteSpace(replacementProfileId))
        {
            return false;
        }

        EmulatorMockData data = CurrentData;
        EmulatorSessionPlayerData existingPlayer = data.Session.Players.FirstOrDefault(sessionPlayer => sessionPlayer.SessionId == targetSessionId);
        if(existingPlayer == null)
        {
            return false;
        }

        EmulatorProfileData replacementProfile = data.Profiles.FirstOrDefault(profile =>
            string.Equals(profile?.PlayerId, replacementProfileId, StringComparison.Ordinal));
        if(replacementProfile == null)
        {
            return false;
        }

        if(data.Session.Players.Any(sessionPlayer =>
            sessionPlayer.SessionId != targetSessionId
            && string.Equals(sessionPlayer?.PlayerId, replacementProfileId, StringComparison.Ordinal)))
        {
            return false;
        }

        existingPlayer.PlayerId = replacementProfile.PlayerId;
        existingPlayer.Type = BoardPlayerType.Profile;
        existingPlayer.DisplayName = null;
        existingPlayer.AvatarId = null;
        existingPlayer.Avatar = null;
        existingPlayer.AvatarBackgroundColor = default;

        PersistAndNotify(raisePlayersChanged: true);
        return true;
    }

    /// <inheritdoc />
    public bool SetActiveProfile(string playerId)
    {
        if(string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        EmulatorMockData data = CurrentData;
        if(data.Profiles.Any(profile => string.Equals(profile.PlayerId, playerId, StringComparison.Ordinal)) == false)
        {
            return false;
        }

        if(string.Equals(data.Session.ActiveProfileId, playerId, StringComparison.Ordinal))
        {
            return true;
        }

        data.Session.ActiveProfileId = playerId;
        PersistAndNotify(raiseActiveProfileChanged: true);
        return true;
    }

    /// <inheritdoc />
    public bool CreateProfile(string displayName, Color avatarBackgroundColor)
    {
        string normalizedDisplayName = NormalizeProfileDisplayName(displayName);
        if(IsValidProfileDisplayName(normalizedDisplayName) == false)
        {
            return false;
        }

        EmulatorMockData data = CurrentData;
        if(IsProfileDisplayNameAvailable(data, normalizedDisplayName) == false)
        {
            return false;
        }

        HashSet<string> assignedPlayerIds = data.Profiles
            .Select(profile => profile?.PlayerId)
            .Where(playerId => string.IsNullOrWhiteSpace(playerId) == false)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> assignedAvatarIds = data.Profiles
            .Select(profile => profile?.AvatarId)
            .Where(avatarId => string.IsNullOrWhiteSpace(avatarId) == false)
            .ToHashSet(StringComparer.Ordinal);

        int profileCountBeforeCreate = data.Profiles.Count;
        data.Profiles.Add(new EmulatorProfileData
        {
            PlayerId = AllocateProfileId(assignedPlayerIds, profileCountBeforeCreate + 1),
            DisplayName = normalizedDisplayName,
            AvatarId = AllocateAvatarId(assignedAvatarIds),
            Type = BoardPlayerType.Profile,
            AvatarBackgroundColor = EmulatorAvatarUtility.HasSerializedColor(avatarBackgroundColor)
                ? avatarBackgroundColor
                : ResolveDefaultAvatarBackgroundColor(profileCountBeforeCreate, avatarId: null)
        });

        PersistAndNotify();
        return true;
    }

    /// <inheritdoc />
    public bool UpdateProfile(string playerId, string displayName, Color avatarBackgroundColor)
    {
        if(string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        string normalizedDisplayName = NormalizeProfileDisplayName(displayName);
        if(IsValidProfileDisplayName(normalizedDisplayName) == false)
        {
            return false;
        }

        if(IsProfileDisplayNameAvailable(CurrentData, normalizedDisplayName, excludedPlayerId: playerId) == false)
        {
            return false;
        }

        EmulatorProfileData profile = CurrentData.Profiles.FirstOrDefault(candidate =>
            string.Equals(candidate?.PlayerId, playerId, StringComparison.Ordinal));
        if(profile == null)
        {
            return false;
        }

        profile.DisplayName = normalizedDisplayName;
        profile.AvatarBackgroundColor = EmulatorAvatarUtility.HasSerializedColor(avatarBackgroundColor)
            ? avatarBackgroundColor
            : profile.AvatarBackgroundColor;

        PersistAndNotify();
        return true;
    }

    /// <inheritdoc />
    public bool DeleteProfile(string playerId)
    {
        if(string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        EmulatorMockData data = CurrentData;
        int originalProfileCount = data.Profiles.Count;
        string originalActiveProfileId = data.Session.ActiveProfileId;
        int originalSessionPlayerCount = data.Session.Players.Count;
        int originalSaveGameCount = data.SaveGames.Count;
        int originalSaveGamePlayerCount = data.SaveGames.Sum(saveGame => saveGame?.Players?.Count ?? 0);

        data.Profiles.RemoveAll(profile => string.Equals(profile?.PlayerId, playerId, StringComparison.Ordinal));
        if(data.Profiles.Count == originalProfileCount)
        {
            return false;
        }

        data.Session.Players.RemoveAll(player => string.Equals(player?.PlayerId, playerId, StringComparison.Ordinal));
        foreach(EmulatorSaveGameData saveGame in data.SaveGames)
        {
            saveGame?.Players?.RemoveAll(player => string.Equals(player?.PlayerId, playerId, StringComparison.Ordinal));
        }

        data.SaveGames.RemoveAll(saveGame => (saveGame?.Players?.Count ?? 0) == 0);
        Normalize(data);

        bool raisePlayersChanged = data.Session.Players.Count != originalSessionPlayerCount;
        bool raiseActiveProfileChanged = string.Equals(originalActiveProfileId, data.Session.ActiveProfileId, StringComparison.Ordinal) == false;
        bool raiseSaveGamesChanged = data.SaveGames.Count != originalSaveGameCount
            || data.SaveGames.Sum(saveGame => saveGame?.Players?.Count ?? 0) != originalSaveGamePlayerCount;

        PersistAndNotify(
            raisePlayersChanged: raisePlayersChanged,
            raiseActiveProfileChanged: raiseActiveProfileChanged,
            raiseSaveGamesChanged: raiseSaveGamesChanged);
        return true;
    }

    /// <inheritdoc />
    public Task<BoardAppStorageInfo> GetAppStorageInfo()
    {
        long usedStorage = CurrentData.SaveGames.Sum(saveGame => (long)(saveGame.Payload?.Length ?? 0) + (EmulatorMockDataUtility.EncodeTexture(saveGame.CoverImage)?.Length ?? 0));
        long totalStorage = GetMaxAppStorage();
        return Task.FromResult(new BoardAppStorageInfo
        {
            totalStorage = totalStorage,
            usedStorage = usedStorage,
            remainingStorage = Math.Max(0, totalStorage - usedStorage),
            usagePercentage = totalStorage > 0 ? (float)usedStorage / totalStorage : 0f
        });
    }

    /// <inheritdoc />
    public long GetMaxPayloadSize() => CurrentData.Storage.MaxPayloadSize;
    /// <inheritdoc />
    public long GetMaxAppStorage() => CurrentData.Storage.MaxAppStorage;
    /// <inheritdoc />
    public int GetMaxSaveDescriptionLength() => CurrentData.Storage.MaxSaveDescriptionLength;

    /// <inheritdoc />
    public Task<BoardSaveGameMetadata[]> GetSaveGamesMetadata()
    {
        BoardSaveGameMetadata[] metadata = CurrentData.SaveGames
            .OrderByDescending(saveGame => saveGame.UpdatedAt)
            .Select(_objectFactory.CreateSaveGameMetadata)
            .Where(saveGame => saveGame != null)
            .ToArray();
        return Task.FromResult(metadata);
    }

    /// <inheritdoc />
    public Task<BoardSaveGameMetadata> CreateSaveGame(byte[] payload, BoardSaveGameMetadataChange metadataChange)
    {
        if(payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if(metadataChange == null)
        {
            throw new ArgumentNullException(nameof(metadataChange));
        }

        ValidateSaveMetadataChange(metadataChange);

        ulong now = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        EmulatorSaveGameData saveGame = new()
        {
            SaveId = Guid.NewGuid().ToString("N"),
            Description = metadataChange.description,
            CreatedAt = now,
            UpdatedAt = now,
            PlayedTime = metadataChange.playedTime,
            GameVersion = metadataChange.gameVersion,
            CoverImage = metadataChange.coverImage,
            Payload = payload.ToArray(),
            PayloadChecksum = EmulatorMockDataUtility.ComputeSha256Hex(payload),
            CoverImageChecksum = EmulatorMockDataUtility.ComputeSha256Hex(EmulatorMockDataUtility.EncodeTexture(metadataChange.coverImage)),
            Players = BuildSaveGamePlayersSnapshot(CurrentData)
        };

        CurrentData.SaveGames.Add(saveGame);
        PersistAndNotify(raiseSaveGamesChanged: true);
        return Task.FromResult(_objectFactory.CreateSaveGameMetadata(saveGame));
    }

    /// <inheritdoc />
    public Task<BoardSaveGameMetadata> UpdateSaveGame(string saveId, byte[] payload, BoardSaveGameMetadataChange metadataChange)
    {
        if(string.IsNullOrWhiteSpace(saveId))
        {
            throw new ArgumentNullException(nameof(saveId));
        }

        if(payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if(metadataChange == null)
        {
            throw new ArgumentNullException(nameof(metadataChange));
        }

        ValidateSaveMetadataChange(metadataChange);

        EmulatorSaveGameData saveGame = CurrentData.SaveGames.FirstOrDefault(candidate => string.Equals(candidate.SaveId, saveId, StringComparison.Ordinal));
        if(saveGame == null)
        {
            throw new InvalidOperationException($"Save game <{saveId}> does not exist.");
        }

        saveGame.Description = metadataChange.description;
        saveGame.PlayedTime = metadataChange.playedTime;
        saveGame.GameVersion = metadataChange.gameVersion;
        saveGame.CoverImage = metadataChange.coverImage;
        saveGame.Payload = payload.ToArray();
        saveGame.UpdatedAt = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        saveGame.PayloadChecksum = EmulatorMockDataUtility.ComputeSha256Hex(payload);
        saveGame.CoverImageChecksum = EmulatorMockDataUtility.ComputeSha256Hex(EmulatorMockDataUtility.EncodeTexture(metadataChange.coverImage));
        saveGame.Players = BuildSaveGamePlayersSnapshot(CurrentData);

        PersistAndNotify(raiseSaveGamesChanged: true);
        return Task.FromResult(_objectFactory.CreateSaveGameMetadata(saveGame));
    }

    /// <inheritdoc />
    public Task<byte[]> LoadSaveGame(string saveId)
    {
        if(string.IsNullOrWhiteSpace(saveId))
        {
            throw new ArgumentNullException(nameof(saveId));
        }

        EmulatorSaveGameData saveGame = CurrentData.SaveGames.FirstOrDefault(candidate => string.Equals(candidate.SaveId, saveId, StringComparison.Ordinal));
        if(saveGame == null)
        {
            throw new InvalidOperationException($"Save game <{saveId}> does not exist.");
        }

        RestoreSessionPlayersFromSave(saveGame);
        PersistAndNotify(raisePlayersChanged: true, raiseActiveProfileChanged: true);
        return Task.FromResult(saveGame.Payload?.ToArray() ?? Array.Empty<byte>());
    }

    /// <inheritdoc />
    public Task<Texture2D> LoadSaveGameCoverImage(string saveId)
    {
        if(string.IsNullOrWhiteSpace(saveId))
        {
            throw new ArgumentNullException(nameof(saveId));
        }

        Texture2D coverImage = CurrentData.SaveGames
            .FirstOrDefault(candidate => string.Equals(candidate.SaveId, saveId, StringComparison.Ordinal))
            ?.CoverImage;
        return Task.FromResult(coverImage);
    }

    /// <inheritdoc />
    public Task<bool> RemovePlayersFromSaveGame(string saveId)
    {
        if(string.IsNullOrWhiteSpace(saveId))
        {
            throw new ArgumentNullException(nameof(saveId));
        }

        EmulatorSaveGameData saveGame = CurrentData.SaveGames.FirstOrDefault(candidate => string.Equals(candidate.SaveId, saveId, StringComparison.Ordinal));
        if(saveGame == null)
        {
            throw new InvalidOperationException($"Save game <{saveId}> does not exist.");
        }

        HashSet<string> sessionPlayerIds = CurrentData.Session.Players.Select(player => player.PlayerId).ToHashSet(StringComparer.Ordinal);
        saveGame.Players.RemoveAll(player => sessionPlayerIds.Contains(player.PlayerId));
        if(saveGame.Players.Count == 0)
        {
            CurrentData.SaveGames.Remove(saveGame);
        }

        PersistAndNotify(raiseSaveGamesChanged: true);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> RemoveActiveProfileFromSaveGame(string saveId)
    {
        if(string.IsNullOrWhiteSpace(saveId))
        {
            throw new ArgumentNullException(nameof(saveId));
        }

        EmulatorSaveGameData saveGame = CurrentData.SaveGames.FirstOrDefault(candidate => string.Equals(candidate.SaveId, saveId, StringComparison.Ordinal));
        if(saveGame == null)
        {
            throw new InvalidOperationException($"Save game <{saveId}> does not exist.");
        }

        string activeProfileId = CurrentData.Session.ActiveProfileId;
        saveGame.Players.RemoveAll(player => string.Equals(player.PlayerId, activeProfileId, StringComparison.Ordinal));
        if(saveGame.Players.Count == 0)
        {
            CurrentData.SaveGames.Remove(saveGame);
        }

        PersistAndNotify(raiseSaveGamesChanged: true);
        return Task.FromResult(true);
    }

    private BoardSessionPlayer[] BuildPlayers()
    {
        return CurrentData.Session.Players
            .Select(sessionPlayer => _objectFactory.CreateSessionPlayer(sessionPlayer, ResolveSessionPlayerProfile(CurrentData, sessionPlayer)))
            .Where(player => player != null)
            .ToArray();
    }

    private BoardPlayer BuildActiveProfile()
    {
        EmulatorMockData data = CurrentData;
        EmulatorProfileData profile = data.Profiles.FirstOrDefault(candidate =>
            string.Equals(candidate.PlayerId, data.Session.ActiveProfileId, StringComparison.Ordinal))
            ?? data.Profiles.FirstOrDefault();
        return _objectFactory.CreateBoardPlayer(profile);
    }

    private static EmulatorProfileData ResolveProfile(EmulatorMockData data, string playerId)
    {
        if(data == null || string.IsNullOrWhiteSpace(playerId))
        {
            return null;
        }

        return data.Profiles.FirstOrDefault(candidate => string.Equals(candidate.PlayerId, playerId, StringComparison.Ordinal));
    }

    private static EmulatorProfileData ResolveSessionPlayerProfile(EmulatorMockData data, EmulatorSessionPlayerData sessionPlayer)
    {
        if(sessionPlayer == null)
        {
            return null;
        }

        EmulatorProfileData persistedProfile = ResolveProfile(data, sessionPlayer.PlayerId);
        if(persistedProfile != null)
        {
            return persistedProfile;
        }

        if(sessionPlayer.Type != BoardPlayerType.Guest)
        {
            return null;
        }

        return new EmulatorProfileData
        {
            PlayerId = sessionPlayer.PlayerId,
            DisplayName = sessionPlayer.DisplayName,
            AvatarId = sessionPlayer.AvatarId,
            Avatar = sessionPlayer.Avatar,
            AvatarBackgroundColor = sessionPlayer.AvatarBackgroundColor,
            Type = BoardPlayerType.Guest
        };
    }

    private static EmulatorProfileData CreateGuestProfile(EmulatorMockData data, string displayName = null, string forcedPlayerId = null)
    {
        int guestIndex = GetNextGuestIndex(data);
        string guestId = forcedPlayerId ?? $"guest-{guestIndex:000}";
        HashSet<string> assignedAvatarIds = data?.Profiles?
            .Select(profile => profile?.AvatarId)
            .Where(avatarId => string.IsNullOrWhiteSpace(avatarId) == false && int.TryParse(avatarId, out _))
            .ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);

        foreach(EmulatorSessionPlayerData sessionPlayer in data?.Session?.Players ?? Enumerable.Empty<EmulatorSessionPlayerData>())
        {
            if(string.IsNullOrWhiteSpace(sessionPlayer?.AvatarId) == false && int.TryParse(sessionPlayer.AvatarId, out _))
            {
                assignedAvatarIds.Add(sessionPlayer.AvatarId);
            }
        }

        string avatarId = AllocateAvatarId(assignedAvatarIds);
        HashSet<string> reservedDisplayNames = data?.Profiles?
            .Select(profile => profile?.DisplayName)
            .Where(existingDisplayName => string.IsNullOrWhiteSpace(existingDisplayName) == false)
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach(EmulatorSessionPlayerData sessionPlayer in data?.Session?.Players ?? Enumerable.Empty<EmulatorSessionPlayerData>())
        {
            if(sessionPlayer?.Type != BoardPlayerType.Guest || string.IsNullOrWhiteSpace(sessionPlayer.DisplayName))
            {
                continue;
            }

            reservedDisplayNames.Add(sessionPlayer.DisplayName);
        }

        string resolvedDisplayName = ResolveUniqueGuestDisplayName(
            NormalizeGuestDisplayName(displayName),
            defaultDisplayName: $"Player {guestIndex}",
            reservedDisplayNames);

        return new EmulatorProfileData
        {
            PlayerId = guestId,
            DisplayName = resolvedDisplayName,
            AvatarId = avatarId,
            AvatarBackgroundColor = ResolveDefaultAvatarBackgroundColor(guestIndex - 1, avatarId),
            Type = BoardPlayerType.Guest
        };
    }

    private static EmulatorSessionPlayerData CreateGuestSessionPlayer(EmulatorMockData data, string displayName = null, string forcedPlayerId = null)
    {
        EmulatorProfileData guestProfile = CreateGuestProfile(data, displayName, forcedPlayerId);
        return new EmulatorSessionPlayerData
        {
            PlayerId = guestProfile.PlayerId,
            Type = BoardPlayerType.Guest,
            DisplayName = guestProfile.DisplayName,
            AvatarId = guestProfile.AvatarId,
            Avatar = guestProfile.Avatar,
            AvatarBackgroundColor = guestProfile.AvatarBackgroundColor
        };
    }

    private static int AllocateSessionId(EmulatorMockData data)
    {
        return data.Session.Players.Select(player => player.SessionId).DefaultIfEmpty(0).Max() + 1;
    }

    private static int GetNextGuestIndex(EmulatorMockData data)
    {
        HashSet<string> playerIds = data.Profiles
            .Select(profile => profile.PlayerId)
            .Where(playerId => string.IsNullOrWhiteSpace(playerId) == false)
            .ToHashSet(StringComparer.Ordinal);

        foreach(EmulatorSessionPlayerData sessionPlayer in data.Session?.Players ?? Enumerable.Empty<EmulatorSessionPlayerData>())
        {
            if(string.IsNullOrWhiteSpace(sessionPlayer?.PlayerId) == false)
            {
                playerIds.Add(sessionPlayer.PlayerId);
            }
        }

        int guestIndex = 1;
        while(playerIds.Contains($"guest-{guestIndex:000}"))
        {
            guestIndex++;
        }

        return guestIndex;
    }

    private static void EnsureActiveProfile(EmulatorMockData data)
    {
        if(data.Profiles.Count == 0)
        {
            data.Profiles.Add(new EmulatorProfileData
            {
                PlayerId = "profile-001",
                DisplayName = "Player 1",
                AvatarId = "0",
                Type = BoardPlayerType.Profile,
                AvatarBackgroundColor = ResolveDefaultAvatarBackgroundColor(profileIndex: 0, avatarId: "0")
            });
        }

        if(string.IsNullOrWhiteSpace(data.Session.ActiveProfileId)
        || data.Profiles.Any(profile => string.Equals(profile.PlayerId, data.Session.ActiveProfileId, StringComparison.Ordinal)) == false)
        {
            data.Session.ActiveProfileId = data.Profiles[0].PlayerId;
        }
    }

    private static void Normalize(EmulatorMockData data)
    {
        data.Application ??= new EmulatorApplicationData();
        data.Profiles ??= new List<EmulatorProfileData>();
        data.Session ??= new EmulatorSessionData();
        data.Session.Players ??= new List<EmulatorSessionPlayerData>();
        data.Storage ??= new EmulatorStorageData();
        data.SaveGames ??= new List<EmulatorSaveGameData>();
        NormalizeProfiles(data);

        EnsureActiveProfile(data);
        NormalizeSessionPlayers(data);
        EnsureSessionContainsNonGuestPlayer(data);
    }

    private static void NormalizeProfiles(EmulatorMockData data)
    {
        HashSet<string> assignedPlayerIds = new(StringComparer.Ordinal);
        HashSet<string> assignedAvatarIds = new(StringComparer.Ordinal);
        for(int i = 0; i < data.Profiles.Count; i++)
        {
            EmulatorProfileData profile = data.Profiles[i] ?? new EmulatorProfileData();
            data.Profiles[i] = profile;

            if(string.IsNullOrWhiteSpace(profile.PlayerId) || assignedPlayerIds.Contains(profile.PlayerId))
            {
                profile.PlayerId = AllocateProfileId(assignedPlayerIds, i + 1);
            }

            assignedPlayerIds.Add(profile.PlayerId);

            if(string.IsNullOrWhiteSpace(profile.DisplayName))
            {
                profile.DisplayName = $"Player{i + 1}";
            }

            if(profile.Type != BoardPlayerType.Profile)
            {
                profile.Type = BoardPlayerType.Profile;
            }

            if(string.IsNullOrWhiteSpace(profile.AvatarId)
            || int.TryParse(profile.AvatarId, out _ ) == false
            || assignedAvatarIds.Contains(profile.AvatarId))
            {
                profile.AvatarId = AllocateAvatarId(assignedAvatarIds);
            }

            assignedAvatarIds.Add(profile.AvatarId);

            if(EmulatorAvatarUtility.HasSerializedColor(profile.AvatarBackgroundColor) == false)
            {
                profile.AvatarBackgroundColor = ResolveDefaultAvatarBackgroundColor(i, profile.AvatarId);
            }
        }
    }

    private static void NormalizeSessionPlayers(EmulatorMockData data)
    {
        List<EmulatorSessionPlayerData> normalizedPlayers = data.Session.Players
            .Where(player => player != null && string.IsNullOrWhiteSpace(player.PlayerId) == false)
            .Where(player => player.Type == BoardPlayerType.Guest
                || data.Profiles.Any(profile => string.Equals(profile.PlayerId, player.PlayerId, StringComparison.Ordinal)))
            .ToList();

        HashSet<int> assignedSessionIds = new();
        HashSet<string> reservedGuestDisplayNames = data.Profiles
            .Select(profile => profile?.DisplayName)
            .Where(displayName => string.IsNullOrWhiteSpace(displayName) == false)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int nextSessionId = 1;
        foreach(EmulatorSessionPlayerData player in normalizedPlayers)
        {
            if(player.SessionId <= 0 || assignedSessionIds.Contains(player.SessionId))
            {
                while(assignedSessionIds.Contains(nextSessionId))
                {
                    nextSessionId++;
                }

                player.SessionId = nextSessionId++;
            }

            assignedSessionIds.Add(player.SessionId);

            if(player.Type != BoardPlayerType.Guest)
            {
                continue;
            }

            int guestIndex = GetGuestIndex(player.PlayerId);
            player.DisplayName = ResolveUniqueGuestDisplayName(
                NormalizeGuestDisplayName(player.DisplayName),
                guestIndex > 0 ? $"Player {guestIndex}" : "Player",
                reservedGuestDisplayNames);
            reservedGuestDisplayNames.Add(player.DisplayName);

            if(string.IsNullOrWhiteSpace(player.AvatarId))
            {
                HashSet<string> assignedAvatarIds = data.Profiles
                    .Select(profile => profile?.AvatarId)
                    .Where(avatarId => string.IsNullOrWhiteSpace(avatarId) == false)
                    .ToHashSet(StringComparer.Ordinal);
                foreach(EmulatorSessionPlayerData otherPlayer in normalizedPlayers.Where(otherPlayer => ReferenceEquals(otherPlayer, player) == false))
                {
                    if(string.IsNullOrWhiteSpace(otherPlayer?.AvatarId) == false)
                    {
                        assignedAvatarIds.Add(otherPlayer.AvatarId);
                    }
                }

                player.AvatarId = AllocateAvatarId(assignedAvatarIds);
            }

            if(EmulatorAvatarUtility.HasSerializedColor(player.AvatarBackgroundColor) == false)
            {
                player.AvatarBackgroundColor = ResolveDefaultAvatarBackgroundColor(Math.Max(0, guestIndex - 1), player.AvatarId);
            }
        }

        data.Session.Players = normalizedPlayers;
    }

    private static void EnsureSessionContainsNonGuestPlayer(EmulatorMockData data)
    {
        if(data.Session.Players.Any(player => player?.Type != BoardPlayerType.Guest))
        {
            return;
        }

        EnsureActiveProfile(data);
        data.Session.Players.Add(new EmulatorSessionPlayerData
        {
            SessionId = AllocateSessionId(data),
            PlayerId = data.Session.ActiveProfileId,
            Type = BoardPlayerType.Profile
        });
    }

    private static int GetGuestIndex(string playerId)
    {
        if(string.IsNullOrWhiteSpace(playerId) || playerId.StartsWith("guest-", StringComparison.Ordinal) == false)
        {
            return 0;
        }

        return int.TryParse(playerId.Substring("guest-".Length), out int guestIndex) ? guestIndex : 0;
    }

    private static string AllocateProfileId(ISet<string> assignedPlayerIds, int startingIndex)
    {
        int profileIndex = Math.Max(1, startingIndex);
        string candidate = $"profile-{profileIndex:000}";
        while(assignedPlayerIds.Contains(candidate))
        {
            profileIndex++;
            candidate = $"profile-{profileIndex:000}";
        }

        return candidate;
    }

    private static string AllocateAvatarId(ISet<string> assignedAvatarIds)
    {
        int avatarIndex = 0;
        string candidate = avatarIndex.ToString();
        while(assignedAvatarIds.Contains(candidate))
        {
            avatarIndex++;
            candidate = avatarIndex.ToString();
        }

        return candidate;
    }

    private void InitializeRuntimeDataFromAsset(EmulatorMockData codeOverride)
    {
        if(_asset.Data == null)
        {
            _runtimeData = EmulatorMockDataUtility.Clone(codeOverride ?? EmulatorDefaults.CreateMockData());
            Normalize(_runtimeData);
            PersistRuntimeDataToAsset();
            return;
        }

        _runtimeData = EmulatorMockDataUtility.Clone(_asset.Data);
        Normalize(_runtimeData);
    }

    private void PersistRuntimeDataToAsset()
    {
        RefreshActiveAssetReference();

        if(_asset == null || _runtimeData == null)
        {
            return;
        }

        EmulatorMockData persistedData = EmulatorMockDataUtility.Clone(_runtimeData);
        persistedData.Application.IsProfileSwitcherVisible = false;
        persistedData.Application.HasPauseScreenContext = false;

        _isWritingAsset = true;
        try
        {
            _asset.Replace(persistedData);
            EmulatorEditorPersistenceUtility.SaveAssetIfPossible(_asset);
        }
        finally
        {
            _isWritingAsset = false;
        }
    }

    private void RefreshRuntimeDataFromAsset()
    {
        if(_asset?.Data == null)
        {
            return;
        }

        bool isProfileSwitcherVisible = _runtimeData?.Application?.IsProfileSwitcherVisible ?? false;
        bool hasPauseScreenContext = _runtimeData?.Application?.HasPauseScreenContext ?? false;

        _runtimeData = EmulatorMockDataUtility.Clone(_asset.Data);
        Normalize(_runtimeData);
        _runtimeData.Application.IsProfileSwitcherVisible = isProfileSwitcherVisible;
        _runtimeData.Application.HasPauseScreenContext = hasPauseScreenContext;
    }

    private void RefreshActiveAssetReference()
    {
        EmulatorMockDataAsset activeAsset = EmulatorProjectSettingsBridge.ActiveMockDataAsset;
        if(ReferenceEquals(_asset, activeAsset))
        {
            return;
        }

        if(_asset != null)
        {
            _asset.Changed -= OnAssetChanged;
        }

        _asset = activeAsset;

        if(_asset == null)
        {
            return;
        }

        _asset.Changed += OnAssetChanged;
        RefreshRuntimeDataFromAsset();
    }

    private List<EmulatorSaveGamePlayerData> BuildSaveGamePlayersSnapshot(EmulatorMockData data)
    {
        return data.Session.Players
            .Select(sessionPlayer => ResolveSessionPlayerProfile(data, sessionPlayer))
            .Where(profile => profile != null)
            .Select(profile => new EmulatorSaveGamePlayerData
            {
                PlayerId = profile.PlayerId,
                DisplayName = profile.DisplayName,
                AvatarId = profile.AvatarId,
                Type = profile.Type,
                Avatar = profile.Avatar,
                AvatarBackgroundColor = profile.AvatarBackgroundColor
            })
            .ToList();
    }

    private void RestoreSessionPlayersFromSave(EmulatorSaveGameData saveGame)
    {
        EmulatorMockData data = CurrentData;
        data.Session.Players.Clear();

        int sessionId = 1;
        foreach(EmulatorSaveGamePlayerData savedPlayer in saveGame.Players)
        {
            EmulatorProfileData matchingProfile = data.Profiles.FirstOrDefault(profile => string.Equals(profile.PlayerId, savedPlayer.PlayerId, StringComparison.Ordinal));
            if(matchingProfile != null)
            {
                data.Session.Players.Add(new EmulatorSessionPlayerData
                {
                    SessionId = sessionId++,
                    PlayerId = matchingProfile.PlayerId,
                    Type = BoardPlayerType.Profile
                });
                continue;
            }

            EmulatorSessionPlayerData guestPlayer = CreateGuestSessionPlayer(data, savedPlayer.DisplayName);
            guestPlayer.SessionId = sessionId++;
            guestPlayer.AvatarId = savedPlayer.AvatarId;
            guestPlayer.Avatar = savedPlayer.Avatar;
            guestPlayer.AvatarBackgroundColor = savedPlayer.AvatarBackgroundColor;
            guestPlayer.DisplayName = savedPlayer.DisplayName;
            data.Session.Players.Add(guestPlayer);
        }

        if(data.Session.Players.Count == 0)
        {
            ResetPlayers();
            return;
        }

        EmulatorSessionPlayerData firstProfilePlayer = data.Session.Players.FirstOrDefault(player => player.Type == BoardPlayerType.Profile)
            ?? data.Session.Players.FirstOrDefault(player => data.Profiles.Any(profile => string.Equals(profile.PlayerId, player.PlayerId, StringComparison.Ordinal)));
        if(firstProfilePlayer != null)
        {
            data.Session.ActiveProfileId = firstProfilePlayer.PlayerId;
            return;
        }

        EnsureActiveProfile(data);
        data.Session.Players.Insert(0, new EmulatorSessionPlayerData
        {
            SessionId = AllocateSessionId(data),
            PlayerId = data.Session.ActiveProfileId,
            Type = BoardPlayerType.Profile
        });
    }

    private static void ValidateSaveMetadataChange(BoardSaveGameMetadataChange metadataChange)
    {
        if(string.IsNullOrWhiteSpace(metadataChange.description))
        {
            throw new ArgumentNullException(nameof(metadataChange.description));
        }

        if(string.IsNullOrWhiteSpace(metadataChange.gameVersion))
        {
            throw new ArgumentNullException(nameof(metadataChange.gameVersion));
        }
    }

    private static string NormalizeProfileDisplayName(string displayName)
    {
        return displayName;
    }

    private static string NormalizeGuestDisplayName(string displayName)
    {
        return displayName?.Trim();
    }

    private static string ResolveUniqueGuestDisplayName(
        string desiredDisplayName,
        string defaultDisplayName,
        ISet<string> reservedDisplayNames)
    {
        if(string.IsNullOrWhiteSpace(desiredDisplayName) == false
        && reservedDisplayNames.Contains(desiredDisplayName) == false)
        {
            return desiredDisplayName;
        }

        string normalizedDefaultDisplayName = NormalizeGuestDisplayName(defaultDisplayName);
        if(string.IsNullOrWhiteSpace(normalizedDefaultDisplayName) == false
        && reservedDisplayNames.Contains(normalizedDefaultDisplayName) == false)
        {
            return normalizedDefaultDisplayName;
        }

        int suffix = 1;
        while(true)
        {
            string candidate = $"Player {suffix}";
            if(reservedDisplayNames.Contains(candidate) == false)
            {
                return candidate;
            }

            suffix++;
        }
    }

    private static bool IsValidProfileDisplayName(string displayName)
    {
        if(string.IsNullOrWhiteSpace(displayName))
        {
            return false;
        }

        if(displayName.Length < MinProfileNameLength || displayName.Length > MaxProfileNameLength)
        {
            return false;
        }

        return displayName.All(char.IsLetterOrDigit);
    }

    private static bool IsProfileDisplayNameAvailable(EmulatorMockData data, string displayName, string excludedPlayerId = null)
    {
        if(data?.Profiles == null)
        {
            return true;
        }

        return data.Profiles.Any(profile =>
            profile != null
            && string.Equals(profile.PlayerId, excludedPlayerId, StringComparison.Ordinal) == false
            && string.Equals(profile.DisplayName, displayName, StringComparison.OrdinalIgnoreCase)) == false;
    }

    private static Color ResolveDefaultAvatarBackgroundColor(int profileIndex, string avatarId)
    {
        if(int.TryParse(avatarId, out int parsedAvatarId))
        {
            return EmulatorAvatarUtility.GetPaletteColor(parsedAvatarId);
        }

        return EmulatorAvatarUtility.GetPaletteColor(profileIndex);
    }

    private void PersistAndNotify(
        bool saveAsset = true,
        bool raisePlayersChanged = false,
        bool raiseActiveProfileChanged = false,
        bool raisePauseScreenContextChanged = false,
        bool raiseSaveGamesChanged = false)
    {
        _logger.Trace()?.Log("Persisting Board OS emulator model changes.");

        if(saveAsset)
        {
            PersistRuntimeDataToAsset();
        }

        Changed?.Invoke(this, EventArgs.Empty);
        if(raisePlayersChanged)
        {
            PlayersChanged?.Invoke(this, EventArgs.Empty);
        }

        if(raiseActiveProfileChanged)
        {
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        if(raisePauseScreenContextChanged)
        {
            PauseScreenContextChanged?.Invoke(this, EventArgs.Empty);
        }

        if(raiseSaveGamesChanged)
        {
            SaveGamesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnAssetChanged(object sender, EventArgs eventArgs)
    {
        if(_isWritingAsset)
        {
            return;
        }

        RefreshRuntimeDataFromAsset();
        PersistAndNotify(
            saveAsset: false,
            raisePlayersChanged: true,
            raiseActiveProfileChanged: true,
            raisePauseScreenContextChanged: true,
            raiseSaveGamesChanged: true);
    }

    private void OnActiveMockDataAssetChanged(object sender, EventArgs eventArgs)
    {
        EmulatorMockDataAsset previousAsset = _asset;
        RefreshActiveAssetReference();

        if(ReferenceEquals(previousAsset, _asset))
        {
            return;
        }

        PersistAndNotify(
            saveAsset: false,
            raisePlayersChanged: true,
            raiseActiveProfileChanged: true,
            raisePauseScreenContextChanged: true,
            raiseSaveGamesChanged: true);
    }
}
}
