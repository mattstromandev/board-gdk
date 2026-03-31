using System;

using BoardGDK.Data;
using BoardGDK.Pieces.Behaviors.Conditions;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Base implementation of <see cref="IPieceBehavior"/> providing default behavior.
/// </summary>
[Serializable]
public abstract class PieceBehavior : IPieceBehavior
{
    [SerializeField]
    [Tooltip("The name of the data provider which stores the settings for this behavior.")]
    // TODO: implement a custom editor for this field that shows available data providers as a dropdown
    // so that the string is not entered manually.
    private string m_dataProviderName;
    
    /// <inheritdoc />
    [field: SerializeField]
    [field: Tooltip("Optional execution order for this behavior. Lower numbers execute first.")]
    public int ExecutionOrder { get; set; }
    
    /// <inheritdoc />
    [field: SerializeReference, SubclassSelector]
    [field: Tooltip("Set of conditions that determine when this behavior is applied.")]
    public IPieceBehaviorCondition[] Conditions { get; private set; } = Array.Empty<IPieceBehaviorCondition>();

    /// <inheritdoc />
    [field: SerializeField]
    [field: Tooltip("Flag indicating whether to override the global conditions for this behavior. If true, the global conditions are ignored and this behavior uses only its own conditions.")]
    public bool OverrideGlobalConditions { get; private set; }

    [SerializeField]
    [Tooltip("The behavior settings to override the global settings with, if any.")]
    private PieceBehaviorSettings m_overrideSettings;

    [Tooltip("The piece settling strategies that will be used for this behavior. Note that these are in addition to any piece settling strategies used at the piece system level, and will not override them. If any of the specified strategies indicate that the piece has not yet settled, the behavior will not be activated.")]
    [SerializeReference, SubclassSelector]
    private IPieceSettlingStrategy[] m_pieceSettlingStrategies = Array.Empty<IPieceSettlingStrategy>();
    
    /// <inheritdoc />
    public bool GlobalSettingsOverridden => OverrideSettings != null;

    /// <inheritdoc />
    public IPieceBehaviorSettings OverrideSettings => m_overrideSettings;

    /// <summary>
    /// The <see cref="IDataProvider"/> which stores the settings for this behavior.
    /// </summary>
    /// <remarks>
    /// Note that this will not be available until after dependency injection has occurred.
    /// </remarks>
    protected IDataProvider DataProvider { get; private set; }

    private PieceSettlingResolver _pieceSettlingResolver;
    private IRahmenLogger _logger;

    public PieceBehavior()
    {
        _pieceSettlingResolver = new PieceSettlingResolver(m_pieceSettlingStrategies);
    }

    /// <inheritdoc />
    public bool HasSettled(IPieceSettlingContext context)
    {
        return _pieceSettlingResolver.HaveSettled(context);
    }

    /// <inheritdoc />
    public bool EvaluateConditions(PieceBehaviorContext context)
    {
        for(int i = 0; i < Conditions.Length; ++i)
        {
            IPieceBehaviorCondition condition = Conditions[i];

            if(condition?.Evaluate(context) == false) { return false; }
        }

        return true;
    }

    /// <inheritdoc />
    public abstract void Place(PieceBehaviorContext context);

    /// <inheritdoc />
    public abstract void Activate(PieceBehaviorContext context);

    /// <inheritdoc />
    public abstract void Update(PieceBehaviorContext context);

    /// <inheritdoc />
    public abstract void Deactivate(PieceBehaviorContext context);

    /// <inheritdoc />
    public abstract void PickUp(PieceBehaviorContext context);

    [Inject]
    private void Injection(
        [NotNull] ILoggerFactory loggerFactory, [NotNull] DiContainer container
    )
    {
        _logger = loggerFactory.Get<LogChannels.PieceBehaviorSystem>(this);
        _pieceSettlingResolver = new PieceSettlingResolver(m_pieceSettlingStrategies);

        if(string.IsNullOrWhiteSpace(m_dataProviderName)) { return; }
        
        IDataProvider dataProvider = container.ResolveId<IDataProvider>(m_dataProviderName);
        if(dataProvider == null)
        {
            _logger.Error()?.Log($"Could not find {nameof(IDataProvider)} with name <{m_dataProviderName}>.");

            return;
        }
        
        DataProvider = dataProvider;
    }
}
}
