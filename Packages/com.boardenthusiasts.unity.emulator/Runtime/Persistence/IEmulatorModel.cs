using System;
using System.Threading.Tasks;

using Board.Core;
using Board.Save;
using Board.Session;

using BE.Emulator.Data;

using UnityEngine;

namespace BE.Emulator.Persistence
{
/// <summary>
/// Mutable in-memory representation of the emulator's mock Board OS state.
/// </summary>
public interface IEmulatorModel
{
    /// <summary>
    /// Raised whenever any emulator state changes.
    /// </summary>
    event EventHandler Changed;
    /// <summary>
    /// Raised whenever the session players change.
    /// </summary>
    event EventHandler PlayersChanged;
    /// <summary>
    /// Raised whenever the active profile changes.
    /// </summary>
    event EventHandler ActiveProfileChanged;
    /// <summary>
    /// Raised whenever the pause screen context changes.
    /// </summary>
    event EventHandler PauseScreenContextChanged;
    /// <summary>
    /// Raised whenever the emulator save game collection changes.
    /// </summary>
    event EventHandler SaveGamesChanged;

    /// <summary>
    /// Gets the current normalized emulator data snapshot.
    /// </summary>
    EmulatorMockData CurrentData { get; }
    /// <summary>
    /// Gets whether the profile switcher is currently visible.
    /// </summary>
    bool IsProfileSwitcherVisible { get; }
    /// <summary>
    /// Gets the current pause screen context projected into Board SDK objects.
    /// </summary>
    BoardPauseScreenContext CurrentPauseScreenContext { get; }
    /// <summary>
    /// Gets the current session players projected into Board SDK objects.
    /// </summary>
    BoardSessionPlayer[] Players { get; }
    /// <summary>
    /// Gets the current active profile projected into a Board SDK object.
    /// </summary>
    BoardPlayer ActiveProfile { get; }

    /// <summary>
    /// Shows the profile switcher overlay in the emulator shell.
    /// </summary>
    void ShowProfileSwitcher();
    /// <summary>
    /// Hides the profile switcher overlay in the emulator shell.
    /// </summary>
    void HideProfileSwitcher();
    /// <summary>
    /// Replaces the current pause screen context.
    /// </summary>
    /// <param name="context">The new pause screen context.</param>
    void SetPauseScreenContext(BoardPauseScreenContext context);
    /// <summary>
    /// Replaces the current pause screen context using the provided field values.
    /// </summary>
    void SetPauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        BoardPauseCustomButton[] customButtons = null,
        BoardPauseAudioTrack[] audioTracks = null);
    /// <summary>
    /// Updates the current pause screen context with the provided field values.
    /// </summary>
    void UpdatePauseScreenContext(
        string applicationName = null,
        bool? showSaveOptionUponExit = null,
        BoardPauseCustomButton[] customButtons = null,
        BoardPauseAudioTrack[] audioTracks = null);
    /// <summary>
    /// Clears the current pause screen context.
    /// </summary>
    void ClearPauseScreenContext();

    /// <summary>
    /// Resets the emulator session to the active profile only.
    /// </summary>
    bool ResetPlayers();
    /// <summary>
    /// Adds a player to the emulator session and persists the updated session state.
    /// </summary>
    /// <param name="playerId">The selected persistent profile identifier when <paramref name="playerType"/> is <see cref="BoardPlayerType.Profile"/>.</param>
    /// <param name="playerType">The type of player to add to the current session.</param>
    /// <returns><see langword="true"/> when the player was added; otherwise, <see langword="false"/>.</returns>
    bool AddSessionPlayer(string playerId, BoardPlayerType playerType);
    /// <summary>
    /// Removes a session player from the emulator and persists the updated session state.
    /// </summary>
    /// <param name="sessionId">The session identifier of the player to remove.</param>
    /// <returns><see langword="true"/> when the player was removed; otherwise, <see langword="false"/>.</returns>
    bool RemoveSessionPlayer(int sessionId);
    /// <summary>
    /// Replaces a session player with an available persistent profile and persists the updated session state.
    /// </summary>
    /// <param name="targetSessionId">The session identifier of the player being replaced.</param>
    /// <param name="replacementProfileId">The persistent profile identifier to place into the target session slot.</param>
    /// <returns><see langword="true"/> when the player was replaced; otherwise, <see langword="false"/>.</returns>
    bool ReplaceSessionPlayer(int targetSessionId, string replacementProfileId);
    /// <summary>
    /// Sets the active profile by player identifier.
    /// </summary>
    /// <param name="playerId">The player identifier to activate.</param>
    /// <returns><see langword="true"/> when the active profile was set; otherwise, <see langword="false"/>.</returns>
    bool SetActiveProfile(string playerId);
    /// <summary>
    /// Creates and persists a new profile in the emulator.
    /// </summary>
    /// <param name="displayName">The profile display name to persist.</param>
    /// <param name="avatarBackgroundColor">The internally persisted avatar background color for the new profile.</param>
    /// <returns><see langword="true"/> when the profile was created; otherwise, <see langword="false"/>.</returns>
    bool CreateProfile(string displayName, Color avatarBackgroundColor);
    /// <summary>
    /// Updates and persists an existing profile in the emulator.
    /// </summary>
    /// <param name="playerId">The identifier of the profile to update.</param>
    /// <param name="displayName">The updated profile display name to persist.</param>
    /// <param name="avatarBackgroundColor">The updated internally persisted avatar background color.</param>
    /// <returns><see langword="true"/> when the profile was updated; otherwise, <see langword="false"/>.</returns>
    bool UpdateProfile(string playerId, string displayName, Color avatarBackgroundColor);
    /// <summary>
    /// Deletes and persists an existing profile in the emulator.
    /// </summary>
    /// <param name="playerId">The identifier of the profile to delete.</param>
    /// <returns><see langword="true"/> when the profile was deleted; otherwise, <see langword="false"/>.</returns>
    bool DeleteProfile(string playerId);

    /// <summary>
    /// Gets current application storage usage information.
    /// </summary>
    Task<BoardAppStorageInfo> GetAppStorageInfo();
    /// <summary>
    /// Gets the maximum supported save payload size in bytes.
    /// </summary>
    long GetMaxPayloadSize();
    /// <summary>
    /// Gets the maximum supported application storage size in bytes.
    /// </summary>
    long GetMaxAppStorage();
    /// <summary>
    /// Gets the maximum allowed save description length.
    /// </summary>
    int GetMaxSaveDescriptionLength();
    /// <summary>
    /// Gets save game metadata for the emulator's current save set.
    /// </summary>
    Task<BoardSaveGameMetadata[]> GetSaveGamesMetadata();
    /// <summary>
    /// Creates a new save game in the emulator.
    /// </summary>
    Task<BoardSaveGameMetadata> CreateSaveGame(byte[] payload, BoardSaveGameMetadataChange metadataChange);
    /// <summary>
    /// Updates an existing save game in the emulator.
    /// </summary>
    Task<BoardSaveGameMetadata> UpdateSaveGame(string saveId, byte[] payload, BoardSaveGameMetadataChange metadataChange);
    /// <summary>
    /// Loads a save game payload from the emulator.
    /// </summary>
    Task<byte[]> LoadSaveGame(string saveId);
    /// <summary>
    /// Loads a save game's cover image from the emulator.
    /// </summary>
    Task<Texture2D> LoadSaveGameCoverImage(string saveId);
    /// <summary>
    /// Removes the current session players from a save game in the emulator.
    /// </summary>
    Task<bool> RemovePlayersFromSaveGame(string saveId);
    /// <summary>
    /// Removes the active profile from a save game in the emulator.
    /// </summary>
    Task<bool> RemoveActiveProfileFromSaveGame(string saveId);
}
}
