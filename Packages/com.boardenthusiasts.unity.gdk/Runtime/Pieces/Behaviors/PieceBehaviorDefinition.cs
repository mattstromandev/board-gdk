using System;
using System.Linq;

using BoardGDK.Pieces.Attributes;
using BoardGDK.Pieces.Behaviors.Conditions;

using JetBrains.Annotations;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Data object defining behaviors and conditions that apply when a physical Piece is on the Board.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece Behavior Definition")]
public class PieceBehaviorDefinition : ScriptableObject, IPieceBehaviorDefinition
{
    /// <inheritdoc />
    public string Name => name;

    /// <inheritdoc />
    [field: Tooltip("Board Pieces with these glyph IDs will match this definition.")]
    [field: SerializeField]
    [field: PieceName]
    public int[] GlyphIDs { get; private set; } = Array.Empty<int>();

    /// <inheritdoc />
    [field: Tooltip("The piece behavior sets that will be processed when Pieces with glyph IDs matching those configured are on the Board.")]
    [field: SerializeField]
    public PieceBehaviorSet[] BehaviorSets { get; private set; } = Array.Empty<PieceBehaviorSet>();

    /// <inheritdoc />
    [field: Tooltip("Any behaviors that will be processed in addition to those defined in the behavior sets when Pieces with glyph IDs matching those configured are on the Board.")]
    [field: SerializeReference, SubclassSelector]
    public IPieceBehavior[] Behaviors { get; private set; } = Array.Empty<IPieceBehavior>();

    /// <inheritdoc />
    [field: Tooltip("The conditions that apply for all behaviors referenced in this definition.")]
    [field: SerializeReference, SubclassSelector]
    public IPieceBehaviorCondition[] GlobalConditions { get; private set; } = Array.Empty<IPieceBehaviorCondition>();

    /// <summary>
    /// Queue this <see cref="PieceBehaviorDefinition"/> for injection. This is necessary to ensure that any dependencies
    /// of the behaviors and conditions defined in this definition are properly injected before they are used.
    /// </summary>
    /// <param name="container">The <see cref="DiContainer"/> context.</param>
    public void QueueForInjection([NotNull] DiContainer container)
    {
        container.QueueForInject(this);
        
        string errMsg;
        
        // Behavior sets
        for(int behaviorSetIndex = 0; behaviorSetIndex < BehaviorSets.Length; ++behaviorSetIndex)
        {
            PieceBehaviorSet behaviorSet = BehaviorSets.ElementAt(behaviorSetIndex);

            if(behaviorSet == null)
            {
                errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(PieceBehaviorSet)} at index <{behaviorSetIndex}> of piece behavior definition <{Name}> is null.";
            #if UNITY_EDITOR
                errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(this)}\">Go to location.</a>";
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
                
                container.QueueForInject(pieceBehavior);
            }
        }
        
        // Individual behaviors
        for(int behaviorIndex = 0; behaviorIndex < Behaviors.Length; ++behaviorIndex)
        {
            IPieceBehavior pieceBehavior = Behaviors.ElementAt(behaviorIndex);
                
            if(pieceBehavior == null)
            {
            #if UNITY_EDITOR
                // errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(IPieceBehavior)} at index <{behaviorIndex}> of piece behavior definition <{Name}> is null.";
                // errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(this)}\">Go to location.</a>";
                // UnityEngine.Debug.LogError(errMsg);
            #endif
                
                continue;
            }
            
            container.QueueForInject(pieceBehavior);
        }
        
        // Global conditions
        for(int conditionIndex = 0; conditionIndex < GlobalConditions.Length; ++conditionIndex)
        {
            IPieceBehaviorCondition condition = GlobalConditions.ElementAt(conditionIndex);
                
            if(condition == null)
            {
                errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(IPieceBehaviorCondition)} at index <{conditionIndex}> of piece behavior definition <{Name}> is null.";
            #if UNITY_EDITOR
                errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(this)}\">Go to location.</a>";
            #endif
                UnityEngine.Debug.LogError(errMsg);
                
                continue;
            }
            
            container.QueueForInject(condition);
        }
    }

#if UNITY_EDITOR
    private DiContainer _container;

    private void OnValidate()
    {
        if(_container == null) { return; }
        
        // TODO: deal with duplicate injection possibility
        
        string errMsg;
        
        // Behavior sets
        for(int behaviorSetIndex = 0; behaviorSetIndex < BehaviorSets.Length; ++behaviorSetIndex)
        {
            PieceBehaviorSet behaviorSet = BehaviorSets.ElementAt(behaviorSetIndex);

            if(behaviorSet == null)
            {
                errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(PieceBehaviorSet)} at index <{behaviorSetIndex}> of piece behavior definition <{Name}> is null.";
            #if UNITY_EDITOR
                errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(this)}\">Go to location.</a>";
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
                
                _container.Inject(pieceBehavior);
            }
        }
        
        // Individual behaviors
        for(int behaviorIndex = 0; behaviorIndex < Behaviors.Length; ++behaviorIndex)
        {
            IPieceBehavior pieceBehavior = Behaviors.ElementAt(behaviorIndex);
                
            if(pieceBehavior == null)
            {
            #if UNITY_EDITOR
                // errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(IPieceBehavior)} at index <{behaviorIndex}> of piece behavior definition <{Name}> is null.";
                // errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(this)}\">Go to location.</a>";
                // UnityEngine.Debug.LogError(errMsg);
            #endif
                
                continue;
            }
            
            _container.Inject(pieceBehavior);
        }
        
        // Global conditions
        for(int conditionIndex = 0; conditionIndex < GlobalConditions.Length; ++conditionIndex)
        {
            IPieceBehaviorCondition condition = GlobalConditions.ElementAt(conditionIndex);
                
            if(condition == null)
            {
                errMsg = $"{nameof(PieceSystemInstaller)}: the {nameof(IPieceBehaviorCondition)} at index <{conditionIndex}> of piece behavior definition <{Name}> is null.";
            #if UNITY_EDITOR
                errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(this)}\">Go to location.</a>";
            #endif
                UnityEngine.Debug.LogError(errMsg);
                
                continue;
            }
            
            _container.Inject(condition);
        }
    }

    [Inject]
    private void Injection([NotNull] DiContainer container)
    {
        _container = container;
    }

#endif
}
}
