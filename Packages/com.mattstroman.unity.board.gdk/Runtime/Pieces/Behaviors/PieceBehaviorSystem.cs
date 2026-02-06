using System.Collections.Generic;
using System.Linq;

using Board.Input;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Default implementation of <see cref="IPieceBehaviorSystem"/> that processes piece behaviors based on board contacts.
/// </summary>
public class PieceBehaviorSystem : MonoBehaviour, IPieceBehaviorSystem
{
    /// <inheritdoc />
    public IPieceBehaviorSettings GlobalSettings { get; private set; }

    /// <inheritdoc />
    public IPieceBehaviorPrioritySettings PrioritySettings { get; private set; }

    private IPieceSystem _pieceSystem;

    public void ProcessBoardContact(BoardContact boardContact, IVirtualPiece virtualPiece)
    {
        IPieceBehaviorDefinition[] matchingBehaviorDefinitions = _pieceSystem.ActivePieceSetDefinition.PieceBehaviorDefinitions
            .Where(x => x.GlyphIDs.Contains(boardContact.glyphId))
            .ToArray();

        PieceBehaviorContext behaviorContext = new()
        {
            ActiveContact = boardContact,
            VirtualPiece = virtualPiece
        };
        
        foreach(IPieceBehaviorDefinition matchingPieceDefinition in matchingBehaviorDefinitions)
        {
            behaviorContext.BehaviorDefinition = matchingPieceDefinition;
            behaviorContext.MeetsGlobalConditions = EvaluateGlobalConditions(matchingPieceDefinition, behaviorContext);

            IEnumerable<IPieceBehavior> pieceBehaviorsFromSets
                = matchingPieceDefinition.BehaviorSets.SelectMany(set => set.Behaviors);
            IEnumerable<IPieceBehavior> allPieceBehaviors =
                pieceBehaviorsFromSets.Concat(matchingPieceDefinition.Behaviors);

            // TODO: handle ability to set and order processing by priorities

            foreach(IPieceBehavior pieceBehavior in allPieceBehaviors.OrderBy(x => x.ExecutionOrder))
            {
                pieceBehavior.ProcessContact(behaviorContext);
            }
        }
    }

    [Inject]
    private void Injection(
        IPieceSystem pieceSystem, IPieceBehaviorSettings globalSettings, IPieceBehaviorPrioritySettings prioritySettings
    )
    {
        _pieceSystem = pieceSystem;
        GlobalSettings = globalSettings;
        PrioritySettings = prioritySettings;
    }

    private bool EvaluateGlobalConditions(IPieceBehaviorDefinition pieceBehaviorDefinition, PieceBehaviorContext context)
    {
        return pieceBehaviorDefinition.GlobalConditions.Aggregate(
            true
          , (shouldProcess, condition) => shouldProcess && condition.Evaluate(context)
        );
    }
}
}
