using System;
using System.Collections.Generic;
using System.Linq;

using Board.Input;

using BoardGDK.Extensions.Pieces;
using BoardGDK.Pieces.Behaviors;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces
{
/// <summary>
/// Default implementation of the <see cref="IPieceSystem"/> interface.
/// </summary>
public class PieceSystem : MonoBehaviour, IPieceSystem
{
    private IInstantiator _instantiator;
    private IPieceBehaviorSystem _pieceBehaviorSystem;
    private IPieceSetDefinition[] _availablePieceSetDefinitions;
    private readonly Dictionary<int, VirtualPiece> _activePiecesByBoardContactID = new();

    /// <inheritdoc />
    // TODO: Make this visible as read only in the inspector for debugging purposes
    public IPieceSetDefinition ActivePieceSetDefinition { get; private set; }

    /// <inheritdoc />
    public event Action<IPieceSetDefinition> PieceSetChanged;

    /// <inheritdoc />
    public void ChangePieceSet(IPieceSetDefinition newPieceSet)
    {
        if(newPieceSet == null)
        {
            throw new ArgumentNullException(nameof(newPieceSet));
        }
        
        if(BoardInput.settings == newPieceSet.InputSettings)
        {
            ActivePieceSetDefinition = newPieceSet;
            return;
        }
        
        // TODO: gracefully handle resetting pieces on board when changing piece sets so that behaviors
        // can have a chance to clean up
        BoardInput.settings = newPieceSet.InputSettings;
    }

    /// <inheritdoc />
    public bool IsOnBoard(int glyphID) { return TryGetPiecesOnBoard(glyphID, out _); }

    /// <inheritdoc />
    public bool IsOnBoard(PieceBehaviorDefinition pieceBehaviorDefinition)
    {
        return TryGetPiecesOnBoard(pieceBehaviorDefinition, out _);
    }

    /// <inheritdoc />
    public bool IsTouched(int glyphID)
    {
        bool isPieceOnBoard = TryGetPiecesOnBoard(glyphID, out VirtualPiece[] pieces);
        
        if(isPieceOnBoard == false) { return false; }
        
        return pieces.Any(piece => piece.IsTouched());
    }

    /// <inheritdoc />
    public bool IsTouched(PieceBehaviorDefinition pieceBehaviorDefinition)
    {
        bool isPieceOnBoard = TryGetPiecesOnBoard(pieceBehaviorDefinition, out VirtualPiece[] pieces);
        
        if(isPieceOnBoard == false) { return false; }
        
        return pieces.Any(piece => piece.IsTouched());
    }

    /// <inheritdoc />
    public bool TryGetPiecesOnBoard(int glyphID, out VirtualPiece[] pieces)
    {
        pieces = GetPiecesOnBoard(glyphID);

        return pieces.Length > 0;
    }

    /// <inheritdoc />
    public bool TryGetPiecesOnBoard(PieceBehaviorDefinition pieceBehaviorDefinition, out VirtualPiece[] pieces)
    {
        pieces = GetPiecesOnBoard(pieceBehaviorDefinition);

        return pieces.Length > 0;
    }

    protected VirtualPiece[] GetPiecesOnBoard(int glyphID)
    {
        return _activePiecesByBoardContactID.Values.Where(piece => piece.GlyphID == glyphID).ToArray();
    }

    protected VirtualPiece[] GetPiecesOnBoard(PieceBehaviorDefinition pieceBehaviorDefinition)
    {
        return _activePiecesByBoardContactID.Values
            .Where(piece => pieceBehaviorDefinition.GlyphIDs.Contains(piece.GlyphID)).ToArray();
    }

    [Inject]
    private void Injection(
        IInstantiator instantiator, IPieceBehaviorSystem pieceBehaviorSystem
      , IPieceSetDefinition[] availablePieceSetDefinitions
    )
    {
        _instantiator = instantiator;
        _pieceBehaviorSystem = pieceBehaviorSystem;

        if(availablePieceSetDefinitions.Length == 0)
        {
            Debug.LogError($"{nameof(PieceSystem)}: No piece set definitions were injected.");
        }
        
        _availablePieceSetDefinitions = availablePieceSetDefinitions;
    }

    protected virtual void OnEnable()
    {
        BoardInput.settingsChanged += OnBoardInputSettingsChanged;
        _activePiecesByBoardContactID.Clear();
    }

    protected virtual void Start()
    {
        ChangePieceSet(_availablePieceSetDefinitions[0]);
    }

    protected virtual void Update()
    {
        if(_pieceBehaviorSystem == null) { return; }

        foreach(BoardContact boardContact in BoardInput.GetActiveContacts(BoardContactType.Glyph))
        {
            ProcessBoardContact(boardContact);
        }
    }

    protected virtual void OnDisable()
    {
        BoardInput.settingsChanged -= OnBoardInputSettingsChanged;
    }

    protected virtual void ProcessBoardContact(BoardContact boardContact)
    {
        if(boardContact.phase == BoardContactPhase.None) { return; }

        if(_activePiecesByBoardContactID.TryGetValue(boardContact.contactId, out VirtualPiece piece) == false)
        {
            // It's possible the contact was already ended or canceled in the same frame it began, so it would be an
            // unknown piece, but we shouldn't track it.
            if(boardContact.isNoneEndedOrCanceled)
            {
                return;
            }
            
            string pieceName = $"Piece_{boardContact.contactId}";

            if(boardContact.phase == BoardContactPhase.Began)
            {
                Debug.Log(
                    $"{nameof(PieceSystem)}: Creating virtual piece for board contact <{boardContact.contactId}>, glyph <{boardContact.glyphId}>; Phase: <{boardContact.phase}>"
                );

                GameObject instance = _instantiator.CreateEmptyGameObject(pieceName);
                piece = instance.AddComponent<VirtualPiece>();
                piece.BoardContactID = boardContact.contactId;
                piece.GlyphID = boardContact.glyphId;
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(PieceSystem)}: Unexpected phase for unknown board contact <{boardContact.contactId}>, glyph <{boardContact.glyphId}>; Phase: <{boardContact.phase}>"
                );
                
                // The piece has somehow been orphaned and we need to relocate it.
                piece = FindObjectsByType<VirtualPiece>(FindObjectsSortMode.InstanceID)
                    .SingleOrDefault(p => p.name == pieceName);

                if(piece == null)
                {
                    Debug.LogError(
                        $"{nameof(PieceSystem)}: a virtual piece for board contact <{boardContact.contactId}>, glyph <{boardContact.glyphId}> does not exist, but the phase is in a state when it should; Phase: <{boardContact.phase}>"
                    );

                    return;
                }
                
                Debug.Log(
                    $"{nameof(PieceSystem)}: relocated existing virtual piece for board contact <{boardContact.contactId}>, glyph <{boardContact.glyphId}>; Phase: <{boardContact.phase}>"
                );
            }

            _activePiecesByBoardContactID.Add(boardContact.contactId, piece);
        }

        _pieceBehaviorSystem.ProcessBoardContact(boardContact, piece);

        if(boardContact.isNoneEndedOrCanceled)
        {
            Debug.Log(
                $"{nameof(PieceSystem)}: Destroying virtual piece for board contact <{boardContact.contactId}> for glyph <{boardContact.glyphId}>; Phase: <{boardContact.phase}>"
            );
            _activePiecesByBoardContactID.Remove(boardContact.contactId);
            Destroy(piece.gameObject);
        }
    }

    private void OnBoardInputSettingsChanged()
    {
        IPieceSetDefinition matchingPieceSet = _availablePieceSetDefinitions
            .SingleOrDefault(
                set => set.InputSettings == BoardInput.settings
            );

        if(matchingPieceSet == null)
        {
            Debug.LogError($"{nameof(PieceSystem)}: No matching {nameof(IPieceSetDefinition)} found for the current {nameof(BoardInput)} settings: {JsonUtility.ToJson(BoardInput.settings)}.");
            
            return;
        }
        
        ActivePieceSetDefinition = matchingPieceSet;
        PieceSetChanged?.Invoke(ActivePieceSetDefinition);
    }
}
}
