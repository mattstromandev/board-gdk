using System.Threading.Tasks;

using Board.Save;

using BoardGDK.BoardAdapters;

using BE.Emulator.Persistence;

using JetBrains.Annotations;

using UnityEngine;

using Zenject;

namespace BE.Emulator.Services
{
/// <summary>
/// Runtime service that exposes the save-game-facing Board facade through either the emulator model or live SDK API.
/// </summary>
internal sealed class BoardSaveGameManagerService : IBoardSaveGameManager
{
    private readonly IEmulatorModel _model;

    /// <summary>
    /// Creates the save game manager service.
    /// </summary>
    /// <param name="model">Optional editor-only emulator model backing the service.</param>
    public BoardSaveGameManagerService([InjectOptional] [CanBeNull] IEmulatorModel model = null)
    {
        _model = model;
    }

    /// <inheritdoc />
    public Task<BoardAppStorageInfo> GetAppStorageInfo()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.GetAppStorageInfo();
        }
#endif
        return BoardSaveGameManager.GetAppStorageInfo();
    }

    /// <inheritdoc />
    public long GetMaxPayloadSize()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.GetMaxPayloadSize();
        }
#endif
        return BoardSaveGameManager.GetMaxPayloadSize();
    }

    /// <inheritdoc />
    public long GetMaxAppStorage()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.GetMaxAppStorage();
        }
#endif
        return BoardSaveGameManager.GetMaxAppStorage();
    }

    /// <inheritdoc />
    public int GetMaxSaveDescriptionLength()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.GetMaxSaveDescriptionLength();
        }
#endif
        return BoardSaveGameManager.GetMaxSaveDescriptionLength();
    }

    /// <inheritdoc />
    public Task<BoardSaveGameMetadata[]> GetSaveGamesMetadata()
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.GetSaveGamesMetadata();
        }
#endif
        return BoardSaveGameManager.GetSaveGamesMetadata();
    }

    /// <inheritdoc />
    public Task<BoardSaveGameMetadata> CreateSaveGame(byte[] payload, BoardSaveGameMetadataChange metadataChange)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.CreateSaveGame(payload, metadataChange);
        }
#endif
        return BoardSaveGameManager.CreateSaveGame(payload, metadataChange);
    }

    /// <inheritdoc />
    public Task<BoardSaveGameMetadata> UpdateSaveGame(string saveId, byte[] payload, BoardSaveGameMetadataChange metadataChange)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.UpdateSaveGame(saveId, payload, metadataChange);
        }
#endif
        return BoardSaveGameManager.UpdateSaveGame(saveId, payload, metadataChange);
    }

    /// <inheritdoc />
    public Task<byte[]> LoadSaveGame(string saveId)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.LoadSaveGame(saveId);
        }
#endif
        return BoardSaveGameManager.LoadSaveGame(saveId);
    }

    /// <inheritdoc />
    public Task<Texture2D> LoadSaveGameCoverImage(string saveId)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.LoadSaveGameCoverImage(saveId);
        }
#endif
        return BoardSaveGameManager.LoadSaveGameCoverImage(saveId);
    }

    /// <inheritdoc />
    public Task<bool> RemovePlayersFromSaveGame(string saveId)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.RemovePlayersFromSaveGame(saveId);
        }
#endif
        return BoardSaveGameManager.RemovePlayersFromSaveGame(saveId);
    }

    /// <inheritdoc />
    public Task<bool> RemoveActiveProfileFromSaveGame(string saveId)
    {
#if UNITY_EDITOR
        if(_model != null)
        {
            return _model.RemoveActiveProfileFromSaveGame(saveId);
        }
#endif
        return BoardSaveGameManager.RemoveActiveProfileFromSaveGame(saveId);
    }
}
}
