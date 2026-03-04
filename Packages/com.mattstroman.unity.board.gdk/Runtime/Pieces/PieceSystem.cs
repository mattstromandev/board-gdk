using System;
using System.Collections.Generic;
using System.Linq;

using Board.Input;

using BoardGDK.Pieces.Behaviors;
using BoardGDK.Pieces.Events;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;

using Zenject;

using Object = UnityEngine.Object;

namespace BoardGDK.Pieces
{
/// <summary>
/// Default implementation of the <see cref="IPieceSystem"/> interface.
/// </summary>
public class PieceSystem : IPieceSystem, ITickable
{
    private readonly IInstantiator _instantiator;
    private readonly IPieceBehaviorSystem _pieceBehaviorSystem;
    private readonly IPieceSetDefinition[] _availablePieceSetDefinitions;
    private readonly IPieceSettlingStrategy[] _pieceSettlingStrategies;
    private readonly IRahmenLogger _logger;
    private readonly Dictionary<(int contactID, int glyphID), PieceTrackingContext> _pieceTrackingContextMap = new();
    private readonly PieceSettlingResolver _pieceSettlingResolver;
    private Transform _virtualPieceContainer;

    /// <inheritdoc />
    // TODO: Make this visible as read only in the inspector for debugging purposes
    public IPieceSetDefinition ActivePieceSetDefinition { get; private set; }

    public event EventHandler<IPieceSetDefinitionEvent> PieceSetChanged;
    public event EventHandler<IPieceEvent> PiecePlaced;
    public event EventHandler<IPieceEvent> PiecePickedUp;

    public PieceSystem([NotNull] ILoggerFactory loggerFactory, [NotNull] IInstantiator instantiator, [NotNull] IPieceBehaviorSystem pieceBehaviorSystem,
        [NotNull] IPieceSettlingStrategy[] pieceSettlingStrategies, [NotNull] IPieceSetDefinition[] availablePieceSetDefinitions
    )
    {
        _logger = loggerFactory.Get<LogChannels.PieceSystem>(this);
        _instantiator = instantiator;
        _pieceBehaviorSystem = pieceBehaviorSystem;
        _availablePieceSetDefinitions = availablePieceSetDefinitions ?? Array.Empty<IPieceSetDefinition>();
        _pieceSettlingResolver = new PieceSettlingResolver(pieceSettlingStrategies);
        
        _logger.Trace()?.Log("Initializing...");
        
        BoardInput.settingsChanged += OnBoardInputSettingsChanged;
        ChangePieceSet(_availablePieceSetDefinitions[0]);
    }

    /// <inheritdoc />
    public void ChangePieceSet(IPieceSetDefinition newPieceSet)
    {
        if(newPieceSet == null)
        {
            throw new ArgumentNullException(nameof(newPieceSet));
        }
        
        _logger.Trace()?.Log($"Handling request to change piece set to <{newPieceSet.PieceSetName}>.");
        
        // Board may already have these settings, but we need to sync up
        if(BoardInput.settings == newPieceSet.InputSettings && ActivePieceSetDefinition != newPieceSet)
        {
            _logger.Info()?.Log($"Board's input settings were already set to <{newPieceSet.PieceSetName}>; syncing the piece system.");
            
            SetPieceSet(newPieceSet);
            
            return;
        }
        
        // We tell Board to change, then Board confirms with us via the settings changed event, at which point we change
        // our piece set and confirm the change with the PieceSetChanged event. This way, Board is always the source of
        // truth for what the current settings are, and we only change our piece set in response to that, which keeps
        // everything nicely in sync.
        _logger.Info()?.Log($"Asking Board to set input settings to <{newPieceSet.PieceSetName}>.");
        
        BoardInput.settings = newPieceSet.InputSettings;
    }

    /// <inheritdoc />
    public bool TryGetPiecesOnBoard(int glyphID, out IVirtualPiece[] pieces)
    {
        pieces = GetPiecesOnBoard(glyphID);

        return pieces.Length > 0;
    }

    /// <inheritdoc />
    public bool TryGetPiecesOnBoard(IPieceBehaviorDefinition pieceBehaviorDefinition, out IVirtualPiece[] pieces)
    {
        pieces = GetPiecesOnBoard(pieceBehaviorDefinition);

        return pieces.Length > 0;
    }

    /// <inheritdoc />
    public virtual void Tick()
    {
        // May not have a matching active piece set
        if(ActivePieceSetDefinition == null) { return; }

        foreach(BoardContact boardContact in BoardInput.GetActiveContacts(BoardContactType.Glyph))
        {
            ProcessBoardContact(boardContact);
        }
    }

    protected IVirtualPiece[] GetPiecesOnBoard(int glyphID)
    {
        return _pieceTrackingContextMap.Values.Where(context => context.GlyphID == glyphID && context.HasSettled)
            .Select(context => context.VirtualPiece).ToArray();
    }

    protected IVirtualPiece[] GetPiecesOnBoard(IPieceBehaviorDefinition pieceBehaviorDefinition)
    {
        return _pieceTrackingContextMap.Values
            .Where(context => pieceBehaviorDefinition.GlyphIDs.Contains(context.GlyphID) && context.HasSettled)
            .Select(context => context.VirtualPiece).ToArray();
    }

    protected virtual void ProcessBoardContact(BoardContact contact)
    {
        (int contactID, int glyphID) trackingKey = (contact.contactId, contact.glyphId);
        _logger.Trace()?.Log($"Processing board contact <{trackingKey}> in phase <{contact.phase}> at position <{contact.screenPosition}>.");
        
        if(_pieceTrackingContextMap.TryGetValue(trackingKey, out PieceTrackingContext trackingContext) == false)
        {
            // It's possible the contact was already ended or canceled in the same frame it began, in which case we don't need to do anything
            if(contact.isNoneEndedOrCanceled)
            {
                _logger.Trace()?.Log($"Ignoring new board contact <{trackingKey}> because its phase is already <{contact.phase}> in the same frame it was observed as new.");
                
                return;
            }
            
            if(contact.phase != BoardContactPhase.Began)
            {
                // We should only be seeing new contacts in the Began phase, so if we see one in a different phase, something has gone seriously wrong.
                _logger.Error()?.Log(
                    $"Ignoring board contact <{trackingKey}> due to an unexpected initial phase of <{contact.phase}>. The contact should have been observed in the Began phase, so this may indicate an issue with the {nameof(BoardInput)} system or a desynchronization between the it and this system."
                );
                
                return;
            }
            
            _logger.Debug()?.Log($"Tracking new board contact <{trackingKey}>; initial screen position: {contact.screenPosition}. Piece placement pending settlement.");
            
            trackingContext = new PieceTrackingContext
            {
                NumFramesActive = 1,
                InitialScreenPosition = contact.screenPosition
            };
            _pieceTrackingContextMap.Add(trackingKey, trackingContext);
        }

        trackingContext.Contact = contact;
        if(trackingContext.VirtualPiece is VirtualPiece castVirtualPiece)
        {
            castVirtualPiece.BoardContact = trackingContext.Contact;
        }
        
        if(trackingContext.HasSettled == false)
        {
            if(contact.isNoneEndedOrCanceled)
            {
                _logger.Debug()?.Log($"Board contact <{trackingKey}> failed to settle before ending with phase <{contact.phase}>. Ending contact tracking.");
                _pieceTrackingContextMap.Remove(trackingKey);
                
                return;
            }
            
            _logger.Trace()?.Log($"Processing piece settling strategies for <{trackingKey}> in phase <{contact.phase}>.");
            
            trackingContext.HasSettled = _pieceSettlingResolver.HaveSettled(trackingContext);

            if(trackingContext.HasSettled == false)
            {
                _logger.Trace()?.Log($"Board contact <{trackingKey}> has not settled yet.");
                ++trackingContext.NumFramesActive;

                return;
            }

            bool isKnownPieceName = ActivePieceSetDefinition.GlyphIDMapping.TryGetValue(trackingKey.glyphID, out string pieceName);
            string virtualPieceName = $"{(isKnownPieceName ? pieceName : "PieceNameUnknown")} {trackingKey}";
            _logger.Info()?.Log($"Board contact <{trackingKey}> has settled after <{trackingContext.NumFramesActive}> frames. Placing piece <{virtualPieceName}>.");
            
            GameObject instance = _instantiator.CreateEmptyGameObject(virtualPieceName);
            VirtualPiece piece = instance.AddComponent<VirtualPiece>();
            piece.transform.SetParent(_virtualPieceContainer);
            piece.BoardContact = trackingContext.Contact;
            trackingContext.VirtualPiece = piece;

            _pieceBehaviorSystem.Place(trackingContext);
            PiecePlaced?.Invoke(this, new PieceEvent
            {
                VirtualPiece = trackingContext.VirtualPiece
            });
        }
        
        _logger.Trace()?.Log($"Processing piece behaviors for <{trackingKey}> in phase <{contact.phase}>.");

        _pieceBehaviorSystem.Update(trackingContext);

        if(contact.isNoneEndedOrCanceled)
        {
            _pieceBehaviorSystem.PickUp(trackingContext);
            _pieceTrackingContextMap.Remove(trackingKey);
            PiecePickedUp?.Invoke(this, new PieceEvent
            {
                VirtualPiece = trackingContext.VirtualPiece
            });
            // TODO: Use object pooling for the virtual pieces instead of destroying them.
            Object.Destroy(trackingContext.VirtualPiece.AnchorTransform.gameObject);
            
            _logger.Info()?.Log($"Board contact <{trackingKey}> has ended with phase <{contact.phase}>. Ending contact tracking.");
        }
        
        ++trackingContext.NumFramesActive;
    }

    private void OnBoardInputSettingsChanged()
    {
        _logger.Trace()?.Log($"Handling notification of new Board input settings <{BoardInput.settings.name}>.");
        
        IPieceSetDefinition matchingPieceSet = _availablePieceSetDefinitions
            .SingleOrDefault(set => set.InputSettings == BoardInput.settings);

        if(matchingPieceSet == null)
        {
            _logger.Warning()?.Log($"No matching {nameof(IPieceSetDefinition)} found for new {nameof(BoardInput)} settings <{BoardInput.settings.name}>. The piece system will not process any contacts for this set.");
        }
        else
        {
            _logger.Debug()?.Log($"Matching piece set definition located for <{BoardInput.settings.name}>: <{matchingPieceSet.PieceSetName}>.");
        }
        
        SetPieceSet(matchingPieceSet);
        
        _logger.Trace()?.Log("Notifying observers of piece set change.");
        
        PieceSetChanged?.Invoke(this, new PieceSetDefinitionEvent
        {
            Definition = ActivePieceSetDefinition
        });
    }

    private void SetPieceSet(IPieceSetDefinition newPieceSet)
    {
        if(ActivePieceSetDefinition == newPieceSet)
        {
            _logger.Trace()?.Log($"The active piece set is already <{newPieceSet?.PieceSetName}>.");
            
            return;
        }
        
        if(ActivePieceSetDefinition != null)
        {
            _logger.Info()?.Log($"Cleaning up piece set <{ActivePieceSetDefinition.PieceSetName}>.");

            // Need to clean up any residual tracking contexts because Board does not guarantee that all contacts will
            // end cleanly, and we don't want pieces lingering around after a piece set change.
            foreach(PieceTrackingContext trackingContext in _pieceTrackingContextMap.Values)
            {
                _pieceBehaviorSystem.PickUp(trackingContext);
                Object.Destroy(trackingContext.VirtualPiece.AnchorTransform);
            }
            _pieceTrackingContextMap.Clear();

            if(_virtualPieceContainer != null)
            {
                _logger.Trace()?.Log($"Destroying virtual piece container <{_virtualPieceContainer.name}> for piece set <{ActivePieceSetDefinition.PieceSetName}>.");

                Object.Destroy(_virtualPieceContainer.gameObject);
            }
        }
        
        ActivePieceSetDefinition = newPieceSet;
        
        if(ActivePieceSetDefinition != null)
        {
            _logger.Trace()?.Log($"Creating virtual piece container <{newPieceSet.PieceSetName}> for piece set <{ActivePieceSetDefinition.PieceSetName}>.");
            
            _virtualPieceContainer = _instantiator.CreateEmptyGameObject(newPieceSet.PieceSetName).transform;
        }
        
        _logger.Info()?.Log($"Piece set changed to <{ActivePieceSetDefinition?.PieceSetName}>.");
    }
}
}
