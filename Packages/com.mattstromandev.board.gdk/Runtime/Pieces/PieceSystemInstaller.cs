using System.Collections.Generic;
using System.Linq;

using BoardGDK.Pieces.Behaviors;
using BoardGDK.Pieces.Behaviors.Conditions;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces
{
/// <summary>
/// <see cref="MonoInstaller{TDerived}"/> which installs everything necessary for the <see cref="IPieceSystem"/>.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece System Installer")]
public class PieceSystemInstaller : ScriptableObjectInstaller<PieceSystemInstaller>
{
    [Tooltip("The piece set definitions that will be available for this piece system. The first definition will be the starting active piece set.")]
    [SerializeField]
    private PieceSetDefinition[] m_pieceSetDefinitions;
    
    [Tooltip("(coming soon; not yet implemented) The piece behavior priorities that will be used for this piece system.")]
    [SerializeField]
    private PieceBehaviorPrioritySettings m_pieceBehaviorPrioritySettings;
    
    [SerializeField]
    [Tooltip("The global settings to apply to all piece behaviors.")]
    private PieceBehaviorSettings m_globalPieceBehaviorSettings;

    /// <inheritdoc />
    public override void InstallBindings()
    {
        Container.Bind<IPieceBehaviorSystem>().To<PieceBehaviorSystem>().FromNewComponentOnNewGameObject().AsSingle().WithArguments(
            m_globalPieceBehaviorSettings, m_pieceBehaviorPrioritySettings
        );
        Container.Bind<IPieceSystem>().To<PieceSystem>().FromNewComponentOnNewGameObject().AsSingle()
            .WithArguments<IPieceSetDefinition[]>(m_pieceSetDefinitions).NonLazy();
        
        foreach(PieceSetDefinition pieceSetDefinition in m_pieceSetDefinitions)
        {
            foreach(IPieceBehaviorDefinition pieceBehaviorDefinition in pieceSetDefinition.PieceBehaviorDefinitions)
            {
                foreach(IPieceBehaviorCondition globalCondition in pieceBehaviorDefinition.GlobalConditions)
                {
                    if(globalCondition is ITickable tickable)
                    {
                        Container.Bind<ITickable>().FromInstance(tickable).AsCached();
                    }
                    Container.QueueForInject(globalCondition);
                }
            }

            IEnumerable<IPieceBehavior> allPieceBehaviorsForSet = GetAllPieceBehaviors(pieceSetDefinition);

            foreach(IPieceBehavior pieceBehavior in allPieceBehaviorsForSet)
            {
                foreach(IPieceBehaviorCondition condition in pieceBehavior.Conditions)
                {
                    if(condition is ITickable conditionTickable)
                    {
                        Container.Bind<ITickable>().FromInstance(conditionTickable).AsCached();
                    }
                    Container.QueueForInject(condition);
                }

                if(pieceBehavior is ITickable behaviorTickable)
                {
                    Container.Bind<ITickable>().FromInstance(behaviorTickable).AsCached();
                }
                Container.QueueForInject(pieceBehavior);
            }
        }
    }

    private static IEnumerable<IPieceBehavior> GetAllPieceBehaviors(IPieceSetDefinition pieceSet)
    {
        IEnumerable<IPieceBehavior> pieceBehaviorsFromSets = pieceSet.PieceBehaviorDefinitions
            .SelectMany(x => x.BehaviorSets)
            .SelectMany(x => x.Behaviors);
        IEnumerable<IPieceBehavior> additionalPieceBehaviors =
            pieceSet.PieceBehaviorDefinitions.SelectMany(x => x.Behaviors);

        return pieceBehaviorsFromSets.Concat(additionalPieceBehaviors);
    }
    
    
}
}
