using System;
using System.Collections.Generic;
using System.Linq;

using Board.Input;

using BoardGDK.Pieces.Behaviors;
using BoardGDK.Pieces.Behaviors.Conditions;
using BoardGDK.Pieces.Events;

using Rahmen.Events;
using Rahmen.Logging;

using UnityEngine;
using UnityEngine.Pool;

using Zenject;

namespace BoardGDK.Pieces
{
/// <summary>
/// <see cref="MonoInstaller{TDerived}"/> which installs everything necessary for the <see cref="IPieceSystem"/>.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece System Installer")]
public class PieceSystemInstaller : ScriptableObjectInstaller<PieceSystemInstaller>
{
    [Tooltip("The piece set definitions that will be available for the piece system. The first definition will be the starting active piece set.")]
    [SerializeField]
    private PieceSetDefinition[] m_pieceSetDefinitions = Array.Empty<PieceSetDefinition>();

    [Tooltip("The piece settling strategies that will be used for the piece system.")]
    [SerializeReference, SubclassSelector]
    private IPieceSettlingStrategy[] m_pieceSettlingStrategies = Array.Empty<IPieceSettlingStrategy>();
    
    [Tooltip("(coming soon; not yet implemented) The piece behavior priorities that will be used for the piece system.")]
    [SerializeField]
    private PieceBehaviorPrioritySettings m_pieceBehaviorPrioritySettings;
    
    [SerializeField]
    [Tooltip("The global settings to apply to all piece behaviors.")]
    private PieceBehaviorSettings m_globalPieceBehaviorSettings;

    [SerializeField]
    [Tooltip("All piece set input settings found in the project.")]
    private BoardInputSettings[] m_allBoardInputSettings = Array.Empty<BoardInputSettings>();

    /// <inheritdoc />
    public override void InstallBindings()
    {
        if(Container.HasBinding<ILoggerFactory>() == false)
        {
            throw new ZenjectException(
                $"{nameof(PieceSystemInstaller)}: No binding for {nameof(ILoggerFactory)} was found in the container when installing <{name}>. Please check your dependencies and make sure you have {nameof(Rahmen)} included and properly set up, and that an {nameof(ILoggerFactory)} is properly bound before this installer."
            );
        }
        
        if(Container.HasBinding<IEventSystem>() == false)
        {
            throw new ZenjectException(
                $"{nameof(PieceSystemInstaller)}: No binding for {nameof(IEventSystem)} was found in the container when installing <{name}>. Please check your dependencies and make sure you have {nameof(Rahmen)} {nameof(Rahmen.Events)} included and properly set up, and that an {nameof(IEventSystem)} is properly bound before this installer."
            );
        }

        if(m_pieceSetDefinitions.Length == 0)
        {
            UnityEngine.Debug.LogError($"{nameof(PieceSystemInstaller)}: No piece set definitions were defined for {name}. You must define and include at least one {nameof(PieceSetDefinition)} for the piece system to function.");

            return;
        }

        foreach(IPieceSettlingStrategy pieceSettlingStrategy in m_pieceSettlingStrategies)
        {
            Container.QueueForInject(pieceSettlingStrategy);
        }
        
        Container.BindInstance(m_allBoardInputSettings).AsSingle();
        Container.Bind<IPieceBehaviorSystem>().To<PieceBehaviorSystem>().AsSingle().WithArguments(
            m_globalPieceBehaviorSettings, m_pieceBehaviorPrioritySettings
        );
        Container.BindInterfacesAndSelfTo<PieceSystem>().AsSingle()
            .WithArguments(m_pieceSetDefinitions, m_pieceSettlingStrategies).NonLazy();
        Container.Bind<RahmenEventsAdapter>().AsSingle().NonLazy();
        
        // TODO: change iteration here to also be validation for any null entries that will log messages which can ping
        // the location of where the issue lies.
        
        foreach(PieceSetDefinition pieceSetDefinition in m_pieceSetDefinitions)
        {
            pieceSetDefinition.QueueForInjection(Container);

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
        using PooledObject<List<IPieceBehavior>> _ = UnityEngine.Pool.ListPool<IPieceBehavior>.Get(out List<IPieceBehavior> pieceBehaviorsFromSets);

        string errMsg;
        for(int definitionIndex = 0; definitionIndex < pieceSet.PieceBehaviorDefinitions.Count; ++definitionIndex)
        {
            IPieceBehaviorDefinition behaviorDefinition = pieceSet.PieceBehaviorDefinitions.ElementAt(definitionIndex);

            if(behaviorDefinition == null)
            {
                errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(IPieceBehaviorDefinition)} at index <{definitionIndex}> in piece set <{pieceSet.PieceSetName}> is null.";
            #if UNITY_EDITOR
                if(pieceSet is PieceSetDefinition castPieceSetDefinition)
                {
                    errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(castPieceSetDefinition)}\">Go to location.</a>";
                }
            #endif
                UnityEngine.Debug.LogError(errMsg);
                continue;
            }
            
            for(int behaviorSetIndex = 0; behaviorSetIndex < behaviorDefinition.BehaviorSets.Length; ++behaviorSetIndex)
            {
                PieceBehaviorSet behaviorSet = behaviorDefinition.BehaviorSets.ElementAt(behaviorSetIndex);

                if(behaviorSet == null)
                {
                    errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(PieceBehaviorSet)} at index <{behaviorSetIndex}> of piece behavior definition <{behaviorDefinition.Name}> is null.";
                #if UNITY_EDITOR
                    if(behaviorDefinition is PieceBehaviorDefinition castPieceBehaviorDefinition)
                    {
                        errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(castPieceBehaviorDefinition)}\">Go to location.</a>";
                    }
                #endif
                    UnityEngine.Debug.LogError(errMsg);
                    
                    continue;
                }
                
                for(int behaviorIndex = 0; behaviorIndex < behaviorSet.Behaviors.Length; ++behaviorIndex)
                {
                    IPieceBehavior pieceBehavior = behaviorSet.Behaviors.ElementAt(behaviorIndex);
                    
                    if(pieceBehavior == null)
                    {
                        errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(IPieceBehavior)} at index <{behaviorIndex}> of piece behavior set <{behaviorSet.name}> is null.";
                    #if UNITY_EDITOR
                        errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(behaviorSet)}\">Go to location.</a>";
                    #endif
                        UnityEngine.Debug.LogError(errMsg);
                    
                        continue;
                    }
                    
                    pieceBehaviorsFromSets.Add(pieceBehavior);
                }
            }
            
        }
        
        IEnumerable<IPieceBehavior> additionalPieceBehaviors =
            pieceSet.PieceBehaviorDefinitions.SelectMany(x => x.Behaviors);

        return pieceBehaviorsFromSets.Concat(additionalPieceBehaviors);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        string[] boardInputSettingsGuids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(BoardInputSettings)}");
        string[] boardInputSettingsPaths = boardInputSettingsGuids.Select(UnityEditor.AssetDatabase.GUIDToAssetPath).ToArray();
        m_allBoardInputSettings = boardInputSettingsPaths
            .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<BoardInputSettings>)
            .Where(x => x != null)
            .ToArray();
    }

#endif
}
}
