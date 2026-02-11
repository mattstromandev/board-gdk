using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;
using UnityEngine.Pool;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Default implementation of <see cref="IPieceBehaviorSystem"/> that processes piece behaviors based on board contacts.
/// </summary>
public class PieceBehaviorSystem : IPieceBehaviorSystem
{
    /// <inheritdoc />
    public IPieceBehaviorSettings GlobalSettings { get; }

    /// <inheritdoc />
    public IPieceBehaviorPrioritySettings PrioritySettings { get; }

    // LazyInject avoids a circular dependency between PieceBehaviorSystem and PieceSystem.
    private readonly LazyInject<IPieceSystem> _pieceSystem;
    private readonly IRahmenLogger _logger;
    private readonly Dictionary<((int contactID, int glyphID), IPieceBehavior behavior), PieceBehaviorContext> _behaviorContextMap = new();

    public PieceBehaviorSystem(
        [NotNull] ILoggerFactory loggerFactory, [NotNull] LazyInject<IPieceSystem> pieceSystem,
        [NotNull] IPieceBehaviorSettings globalSettings, [NotNull] IPieceBehaviorPrioritySettings prioritySettings
    )
    {
        _logger = loggerFactory.Get<LogChannels.PieceBehaviorSystem>(this);
        _pieceSystem = pieceSystem;
        GlobalSettings = globalSettings;
        PrioritySettings = prioritySettings;
    }

    /// <inheritdoc />
    public void Place(PieceTrackingContext trackingContext)
    {
        if(_pieceSystem.Value.ActivePieceSetDefinition == null) { return; }
        
        _logger.Trace()?.Log($"Iterating placement for place behaviors for <{trackingContext.TrackingKey}>.");
        IterateBehaviors(trackingContext, PlaceBehavior);
    }

    /// <inheritdoc />
    public void Update(PieceTrackingContext trackingContext)
    {
        if(_pieceSystem.Value.ActivePieceSetDefinition == null) { return; }

        _logger.Trace()?.Log($"Iterating update for behaviors for <{trackingContext.TrackingKey}>.");
        IterateBehaviors(trackingContext, UpdateBehavior);
    }

    /// <inheritdoc />
    public void PickUp(PieceTrackingContext trackingContext)
    {
        if(_pieceSystem.Value.ActivePieceSetDefinition == null) { return; }
        
        _logger.Trace()?.Log($"Iterating pick up behaviors for glyph ID <{trackingContext.GlyphID}>.");
        IterateBehaviors(trackingContext, PickUpBehavior);
    }

    private IEnumerable<IPieceBehaviorDefinition> GetMatchingBehaviorDefinitions(PieceTrackingContext context)
    {
        return _pieceSystem.Value.ActivePieceSetDefinition?.PieceBehaviorDefinitions
            .Where(x => x.GlyphIDs.Contains(context.GlyphID));
    }

    private IEnumerable<IPieceBehavior> GetAllBehaviors(IPieceBehaviorDefinition behaviorDefinition)
    {
        return behaviorDefinition.BehaviorSets.SelectMany(set => set.Behaviors).Concat(behaviorDefinition.Behaviors);
    }

    private bool EvaluateGlobalConditions(IPieceBehaviorDefinition pieceBehaviorDefinition, PieceBehaviorContext context)
    {
        return pieceBehaviorDefinition.GlobalConditions.All(condition => condition.Evaluate(context));
    }
    
    private void IterateBehaviors(PieceTrackingContext context, [NotNull] Action<PieceTrackingContext, IPieceBehaviorDefinition, IPieceBehavior> actionToPerform)
    {
        // TODO: Need to construct the set of ordered behaviors in a way that proper logs can be made for identifying the
        // exact index and list in which a behavior is null to greatly improve debugging.
        IEnumerable<IPieceBehaviorDefinition> matchingBehaviorDefinitions = GetMatchingBehaviorDefinitions(context);
        
        foreach(IPieceBehaviorDefinition matchingPieceDefinition in matchingBehaviorDefinitions)
        {
            IEnumerable<IPieceBehavior> allPieceBehaviors = GetAllBehaviors(matchingPieceDefinition);

            // TODO: handle ability to set and order processing by priorities; consider replacing ExecutionOrder, but it
            // may still be needed in addition to the priority-based processing for finer control in some cases

            IPieceBehavior[] allOrderedBehaviors = allPieceBehaviors.OrderBy(x => x?.ExecutionOrder).ToArray();
            for(int i = 0; i < allOrderedBehaviors.Length; ++i)
            {
                IPieceBehavior behavior = allOrderedBehaviors[i];

                if(behavior == null)
                {
                    _logger.Warning()?.Log($"A piece behavior in piece definition <{matchingPieceDefinition.Name}> is null. You should properly configure or remove this entry.");
                    continue;
                }
                
                actionToPerform(context, matchingPieceDefinition, behavior);
            }
        }
    }

    private void PlaceBehavior([NotNull] PieceTrackingContext trackingContext, [NotNull] IPieceBehaviorDefinition behaviorDefinition, [NotNull] IPieceBehavior behavior)
    {
        if(GetBehaviorContext(trackingContext, behavior, out PieceBehaviorContext behaviorContext))
        {
            _logger.Error()?.Log($"Ignoring the placement request as a behavior context is already being tracked for behavior <{behavior.GetType().Name}> and <{trackingContext.TrackingKey}>, and the piece is already considered placed. This should not happen and likely indicates an issue with the {nameof(PieceSystem)}.");

            return;
        }
        
        _logger.Debug()?.Log($"Tracking new context for behavior <{behavior.GetType().Name}> and <{trackingContext.TrackingKey}>. Behavior activations pending settlement.");

        behaviorContext = new PieceBehaviorContext
        {
            TrackingContext = trackingContext,
            InitialPlacementScreenPosition = trackingContext.Contact.ScreenPosition,
            InitialScreenPosition =  trackingContext.Contact.ScreenPosition,
            BehaviorDefinition = behaviorDefinition,
            State = GenericPool<PieceBehaviorState>.Get()
        };
            
        _behaviorContextMap.Add((trackingContext.TrackingKey, behavior), behaviorContext);
        
        _logger.Notice()?.Log($"Placing piece behavior <{behavior.GetType().Name}> for <{trackingContext.TrackingKey}>.");
        
        behavior.Place(behaviorContext);
    }

    private void UpdateBehavior([NotNull] PieceTrackingContext trackingContext, [NotNull] IPieceBehaviorDefinition behaviorDefinition, [NotNull] IPieceBehavior behavior)
    {
        if(GetBehaviorContext(trackingContext, behavior, out PieceBehaviorContext behaviorContext) == false)
        {
            // TODO: enable live editing in Unity here. This case will be hit in the editor if a developer adds behavior in play mode, and the behavior should be properly placed/activated at that time
        #if UNITY_EDITOR
            if(Application.isPlaying == false) { return; }
            
            behaviorContext = new PieceBehaviorContext
            {
                TrackingContext = trackingContext,
                InitialPlacementScreenPosition = trackingContext.Contact.ScreenPosition,
                InitialScreenPosition =  trackingContext.Contact.ScreenPosition,
                BehaviorDefinition = behaviorDefinition,
                State = GenericPool<PieceBehaviorState>.Get()
            };
            
            _behaviorContextMap.Add((trackingContext.TrackingKey, behavior), behaviorContext);
            
            _logger.Notice()?.Log($"Placing piece behavior <{behavior.GetType().Name}> for <{trackingContext.TrackingKey}>.");
        
            behavior.Place(behaviorContext);
        #else
            _logger.Error()?.Log($"Cannot process behavior <{behavior.GetType().Name}> for <{trackingContext.TrackingKey}> as it has not been placed. The contact should have been placed after settling, so this likely indicates an issue with the {nameof(PieceSystem)}.");
            
            return;
        #endif
        }
        behaviorContext.TrackingContext = trackingContext;
        
        if(EvaluateGlobalConditions(behaviorDefinition, behaviorContext) == false && behavior.OverrideGlobalConditions == false)
        {
            _logger.Notice()?.Log($"Global conditions no longer met for behavior <{behavior.GetType().Name}> and <{behaviorContext.TrackingKey}>. Deactivating behavior.");
            behavior.Deactivate(behaviorContext);
            behaviorContext.IsActive = false;

            return;
        }

        if(behavior.EvaluateConditions(behaviorContext) == false)
        {
            _logger.Notice()?.Log($"Local conditions no longer met for behavior <{behavior.GetType().Name}> and <{behaviorContext.TrackingKey}>. Deactivating behavior.");
            behavior.Deactivate(behaviorContext);
            behaviorContext.IsActive = false;

            return;
        }

        if(behaviorContext.IsActive == false && behaviorContext.HasActivationSettled == true)
        {
            // Need to reset and handle activation settling
            behaviorContext.HasActivationSettled = false;
            behaviorContext.NumFramesActive = 1;
            behaviorContext.InitialScreenPosition = trackingContext.Contact.ScreenPosition;
        }
        
        if(behaviorContext.HasActivationSettled == false)
        {
            if(behaviorContext.Contact.IsNoneEndedOrCanceled)
            {
                _logger.Debug()?.Log($"Piece behavior <{behavior.GetType().Name}> failed to settle for <{trackingContext.TrackingKey}> before it ended with phase <{trackingContext.Contact.Phase}>. Behavior will not be activated.");
                
                // We still need the behavior context for piece pickup, so we don't remove it yet
                
                return;
            }
            
            _logger.Trace()?.Log($"Piece behavior <{behavior.GetType().Name}> processing piece settling strategies for <{trackingContext.TrackingKey}>.");
            
            behaviorContext.HasActivationSettled = behavior.HasSettled(behaviorContext);

            if(behaviorContext.HasActivationSettled == false)
            {
                _logger.Trace()?.Log($"Piece behavior <{behavior.GetType().Name}> has not settled yet for <{trackingContext.TrackingKey}>.");
                ++behaviorContext.NumFramesActive;
                ++behaviorContext.NumFramesPlaced;

                return;
            }
            
            // Behavior has settled and can now be activated
            _logger.Notice()?.Log($"Piece behavior <{behavior.GetType().Name}> has settled for <{trackingContext.TrackingKey}> after <{behaviorContext.NumFramesActive}> frames. Activating behavior.");
            
            behaviorContext.IsActive = true;
            behavior.Activate(behaviorContext);
        }
        
        _logger.Trace()?.Log($"Updating piece behavior <{behavior.GetType().Name}> for <{trackingContext.TrackingKey}>.");
        
        behavior.Update(behaviorContext);
        
        ++behaviorContext.NumFramesActive;
        ++behaviorContext.NumFramesPlaced;
    }

    private void PickUpBehavior([NotNull] PieceTrackingContext trackingContext, [NotNull] IPieceBehaviorDefinition behaviorDefinition, [NotNull] IPieceBehavior behavior)
    {
        if(GetBehaviorContext(trackingContext, behavior, out PieceBehaviorContext behaviorContext) == false)
        {
            // TODO: enable live editing in Unity here. This case will be hit in the editor if a developer has added behavior in play mode, while a piece was placed, and then is picked up.
        #if !UNITY_EDITOR
            _logger.Error()?.Log($"Cannot pick up behavior <{behavior.GetType().Name}> for <{trackingContext.TrackingKey}> as it was never placed. The behavior should have been placed after settling, so this likely indicates an issue with the {nameof(PieceSystem)}.");
        #endif

            return;
        }
        
        _logger.Notice()?.Log($"Picking up piece behavior <{behavior.GetType().Name}> for <{trackingContext.TrackingKey}>.");

        behaviorContext.TrackingContext = trackingContext;
        behavior.PickUp(behaviorContext);
        behaviorContext.State.Clear();
        GenericPool<PieceBehaviorState>.Release((PieceBehaviorState) behaviorContext.State);
        _behaviorContextMap.Remove((trackingContext.TrackingKey, behavior));
    }

    private bool GetBehaviorContext([NotNull] PieceTrackingContext trackingContext, [NotNull] IPieceBehavior behavior, out PieceBehaviorContext behaviorContext)
    {
        return _behaviorContextMap.TryGetValue((trackingContext.TrackingKey, behavior), out behaviorContext);
    }
}
}
