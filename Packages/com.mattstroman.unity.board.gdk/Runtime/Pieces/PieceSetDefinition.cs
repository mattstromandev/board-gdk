using System;
using System.Collections.Generic;

using BoardGDK.Pieces.Behaviors;
using BoardGDK.Pieces.Behaviors.Conditions;

using JetBrains.Annotations;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces
{
/// <summary>
/// <see cref="IPieceSetDefinition"/> exposed to the Unity editor for data-driven piece sets.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece Set Definition")]
public class PieceSetDefinition : ScriptableObject, IPieceSetDefinition
{
    /// <inheritdoc />
    public string PieceSetName => name;

    /// <inheritdoc />
    [field: SerializeField]
    [field: Tooltip("The Board Input Settings associated with this Piece Set.")]
    public PieceSetInputSettings InputSettings { get; private set; }

    [SerializeField]
    [Tooltip("The definitions of Piece Behaviors associated with this Piece Set.")]
    private PieceBehaviorDefinition[] m_pieceBehaviorDefinitions = Array.Empty<PieceBehaviorDefinition>();

    /// <inheritdoc />
    public IReadOnlyDictionary<int, string> GlyphIDMapping => InputSettings.GlyphIDMapping;

    /// <inheritdoc />
    public IReadOnlyCollection<IPieceBehaviorDefinition> PieceBehaviorDefinitions => m_pieceBehaviorDefinitions;

    public void QueueForInjection([NotNull] DiContainer container)
    {
        container.QueueForInject(this);
        
        for(int i = 0; i < m_pieceBehaviorDefinitions.Length; ++i)
        {
            PieceBehaviorDefinition definition = m_pieceBehaviorDefinitions[i];

            if(definition == null)
            {
                string errMsg = $"{nameof(PieceSetDefinition)}: the {nameof(PieceBehaviorCondition)} at index <{i}> of piece set definition <{name}> is null.";
            #if UNITY_EDITOR
                errMsg += $" <a href=\"{UnityEditor.AssetDatabase.GetAssetPath(this)}\">Go to location.</a>";
            #endif
                UnityEngine.Debug.LogError(errMsg);
                
                continue;
            }
            
            definition.QueueForInjection(container);
        }
    }

#if UNITY_EDITOR

    private DiContainer _container;
    
    private void OnValidate()
    {
        if(_container == null) { return; }
        
        // TODO: deal with duplicate injection possibility
        
        QueueForInjection(_container);
    }

    [Inject]
    private void Injection([NotNull] DiContainer container)
    {
        _container = container;
    }

#endif
}
}
