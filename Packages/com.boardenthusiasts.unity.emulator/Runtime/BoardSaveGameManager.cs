using System.Threading.Tasks;

using BE.Emulator.Services;

using UnityEngine;

namespace BE.Emulator
{
/// <summary>
/// Static facade over the emulator's save-game-facing Board surface.
/// </summary>
public static class BoardSaveGameManager
{
    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.GetAppStorageInfo"/>
    public static Task<Board.Save.BoardAppStorageInfo> GetAppStorageInfo()
    {
        return BoardStaticApiRegistry.SaveGameManager.GetAppStorageInfo();
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.GetMaxPayloadSize"/>
    public static long GetMaxPayloadSize()
    {
        return BoardStaticApiRegistry.SaveGameManager.GetMaxPayloadSize();
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.GetMaxAppStorage"/>
    public static long GetMaxAppStorage()
    {
        return BoardStaticApiRegistry.SaveGameManager.GetMaxAppStorage();
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.GetMaxSaveDescriptionLength"/>
    public static int GetMaxSaveDescriptionLength()
    {
        return BoardStaticApiRegistry.SaveGameManager.GetMaxSaveDescriptionLength();
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.GetSaveGamesMetadata"/>
    public static Task<Board.Save.BoardSaveGameMetadata[]> GetSaveGamesMetadata()
    {
        return BoardStaticApiRegistry.SaveGameManager.GetSaveGamesMetadata();
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.CreateSaveGame"/>
    public static Task<Board.Save.BoardSaveGameMetadata> CreateSaveGame(byte[] payload, Board.Save.BoardSaveGameMetadataChange metadataChange)
    {
        return BoardStaticApiRegistry.SaveGameManager.CreateSaveGame(payload, metadataChange);
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.UpdateSaveGame"/>
    public static Task<Board.Save.BoardSaveGameMetadata> UpdateSaveGame(string saveId, byte[] payload, Board.Save.BoardSaveGameMetadataChange metadataChange)
    {
        return BoardStaticApiRegistry.SaveGameManager.UpdateSaveGame(saveId, payload, metadataChange);
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.LoadSaveGame"/>
    public static Task<byte[]> LoadSaveGame(string saveId)
    {
        return BoardStaticApiRegistry.SaveGameManager.LoadSaveGame(saveId);
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.LoadSaveGameCoverImage"/>
    public static Task<Texture2D> LoadSaveGameCoverImage(string saveId)
    {
        return BoardStaticApiRegistry.SaveGameManager.LoadSaveGameCoverImage(saveId);
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.RemovePlayersFromSaveGame"/>
    public static Task<bool> RemovePlayersFromSaveGame(string saveId)
    {
        return BoardStaticApiRegistry.SaveGameManager.RemovePlayersFromSaveGame(saveId);
    }

    /// <inheritdoc cref="global::Board.Save.BoardSaveGameManager.RemoveActiveProfileFromSaveGame"/>
    public static Task<bool> RemoveActiveProfileFromSaveGame(string saveId)
    {
        return BoardStaticApiRegistry.SaveGameManager.RemoveActiveProfileFromSaveGame(saveId);
    }
}
}
