using System.Threading.Tasks;

using Board.Save;

using UnityEngine;

namespace BoardGDK.BoardAdapters
{
/// <summary>
/// Abstraction over <see cref="BoardSaveGameManager"/>.
/// </summary>
public interface IBoardSaveGameManager
{
    /// <inheritdoc cref="BoardSaveGameManager.GetAppStorageInfo"/>
    Task<BoardAppStorageInfo> GetAppStorageInfo();
    /// <inheritdoc cref="BoardSaveGameManager.GetMaxPayloadSize"/>
    long GetMaxPayloadSize();
    /// <inheritdoc cref="BoardSaveGameManager.GetMaxAppStorage"/>
    long GetMaxAppStorage();
    /// <inheritdoc cref="BoardSaveGameManager.GetMaxSaveDescriptionLength"/>
    int GetMaxSaveDescriptionLength();
    /// <inheritdoc cref="BoardSaveGameManager.GetSaveGamesMetadata"/>
    Task<BoardSaveGameMetadata[]> GetSaveGamesMetadata();
    /// <inheritdoc cref="BoardSaveGameManager.CreateSaveGame"/>
    Task<BoardSaveGameMetadata> CreateSaveGame(byte[] payload, BoardSaveGameMetadataChange metadataChange);
    /// <inheritdoc cref="BoardSaveGameManager.UpdateSaveGame"/>
    Task<BoardSaveGameMetadata> UpdateSaveGame(string saveId, byte[] payload, BoardSaveGameMetadataChange metadataChange);
    /// <inheritdoc cref="BoardSaveGameManager.LoadSaveGame"/>
    Task<byte[]> LoadSaveGame(string saveId);
    /// <inheritdoc cref="BoardSaveGameManager.LoadSaveGameCoverImage"/>
    Task<Texture2D> LoadSaveGameCoverImage(string saveId);
    /// <inheritdoc cref="BoardSaveGameManager.RemovePlayersFromSaveGame"/>
    Task<bool> RemovePlayersFromSaveGame(string saveId);
    /// <inheritdoc cref="BoardSaveGameManager.RemoveActiveProfileFromSaveGame"/>
    Task<bool> RemoveActiveProfileFromSaveGame(string saveId);
}
}
