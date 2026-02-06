using System;
using System.Collections.Generic;

using Board.Input;

using BoardGDK.Data;
using BoardGDK.Pieces.Behaviors.Conditions;

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
    
    /// <inheritdoc />
    public bool GlobalSettingsOverridden => OverrideSettings != null;

    /// <inheritdoc />
    public IPieceBehaviorSettings OverrideSettings => m_overrideSettings;

    /// <summary>
    /// Flag indicating whether piece settling is currently enabled for this behavior.
    /// </summary>
    protected bool UsingPieceSettling => GlobalSettingsOverridden ? OverrideSettings.UsePieceSettling : _pieceBehaviorSystem.GlobalSettings.UsePieceSettling;

    /// <summary>
    /// The number of frames to wait for piece settling, if enabled.
    /// </summary>
    protected int PieceSettlingFrames => GlobalSettingsOverridden ? OverrideSettings.PieceSettlingFrames : _pieceBehaviorSystem.GlobalSettings.PieceSettlingFrames;

    /// <summary>
    /// The <see cref="IDataProvider"/> which stores the settings for this behavior.
    /// </summary>
    /// <remarks>
    /// Note that this will not be available until after dependency injection has occurred.
    /// </remarks>
    protected IDataProvider DataProvider { get; private set; }

    private List<int> _activeBehaviorContactIDs = new();
    private readonly Dictionary<int, int> _pendingSettleFramesByContactId = new();
    private IPieceBehaviorSystem _pieceBehaviorSystem;

    /// <inheritdoc />
    public virtual void ProcessContact(PieceBehaviorContext context)
    {
        if(context.MeetsGlobalConditions == false && OverrideGlobalConditions == false)
        {
            // Global conditions no longer met; deactivate if active
            Deactivate(context);

            return;
        }

        bool meetsConditions = EvaluateConditions(context);

        if(meetsConditions == false)
        {
            // Local conditions no longer met; deactivate if active
            Deactivate(context);

            return;
        }

        if(context.ActiveContact.phase == BoardContactPhase.Began && IsBehaviorPendingSettle(context.ActiveContact) == false)
        {
            // New contact; start settling process
            HandleSettle(context);

            return;
        }

        if(context.ActiveContact.isInProgress)
        {
            // Contact is ongoing
            Update(context);
        }
        else if(context.ActiveContact.isNoneEndedOrCanceled)
        {
            // Contact ended (removed from Board); deactivate behavior and inform of removal
            Deactivate(context);
            OnRemoved(context);
        }
    }

    /// <summary>
    /// Add any necessary logic specific to your behavior for when the Piece placement has settled.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    protected virtual void OnPlaced(PieceBehaviorContext context) { }

    /// <summary>
    /// Add any necessary activation logic specific to your behavior.
    /// </summary>
    /// <remarks>
    /// A Piece can be activated and deactivated multiple times while its <see cref="BoardContact"/> is active, depending
    /// on the <see cref="Conditions"/>.
    /// </remarks>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    protected virtual void OnActivate(PieceBehaviorContext context) { }

    /// <summary>
    /// Add any necessary update logic specific to your behavior.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    protected virtual void OnUpdate(PieceBehaviorContext context) { }

    /// <summary>
    /// Add any necessary deactivation logic specific to your behavior.
    /// </summary>
    /// <remarks>
    /// A Piece can be activated and deactivated multiple times while its <see cref="BoardContact"/> is active, depending
    /// on the <see cref="Conditions"/>.
    /// </remarks>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    protected virtual void OnDeactivate(PieceBehaviorContext context) { }

    /// <summary>
    /// Add any necessary logic specific to your behavior for when the Piece has been removed from Board.
    /// </summary>
    /// <param name="context">The necessary context for processing the contact and making decisions about behavior.</param>
    protected virtual void OnRemoved(PieceBehaviorContext context) { }

    [Inject]
    private void Injection(DiContainer container, IPieceBehaviorSystem pieceBehaviorSystem)
    {
        _activeBehaviorContactIDs.Clear();
        _pieceBehaviorSystem = pieceBehaviorSystem;

        if(string.IsNullOrWhiteSpace(m_dataProviderName)) { return; }
        
        IDataProvider dataProvider = container.ResolveId<IDataProvider>(m_dataProviderName);
        if(dataProvider == null)
        {
            UnityEngine.Debug.LogError($"{nameof(PieceBehavior)}: <{GetType().Name}> could not find {nameof(IDataProvider)} with name <{m_dataProviderName}>.");

            return;
        }
        
        DataProvider = dataProvider;
    }

    private bool IsBehaviorActive(BoardContact boardContact)
    {
        return _activeBehaviorContactIDs.Contains(boardContact.contactId);
    }
    
    private bool IsBehaviorPendingSettle(BoardContact boardContact)
    {
        return _pendingSettleFramesByContactId.ContainsKey(boardContact.contactId);
    }

    private void SetBehaviorActive(BoardContact boardContact, bool isActive = true)
    {
        if(isActive)
        {
            _activeBehaviorContactIDs.Add(boardContact.contactId);
        }
        else
        {
            _activeBehaviorContactIDs.Remove(boardContact.contactId);
        }
    }

    private bool EvaluateConditions(PieceBehaviorContext context)
    {
        for(int i = 0; i < Conditions.Length; ++i)
        {
            IPieceBehaviorCondition condition = Conditions[i];

            if(condition?.Evaluate(context) == false) { return false; }
        }

        return true;
    }

    private void HandleSettle(PieceBehaviorContext context)
    {
        if(UsingPieceSettling)
        {
            _pendingSettleFramesByContactId[context.ActiveContact.contactId] = PieceSettlingFrames;
        }
        else
        {
            OnPlaced(context);
            Activate(context);
        }
    }

    private void Activate(PieceBehaviorContext context)
    {
        SetBehaviorActive(context.ActiveContact);
        OnActivate(context);
    }

    private void Update(PieceBehaviorContext context)
    {
        int contactId = context.ActiveContact.contactId;
        if(UsingPieceSettling && _pendingSettleFramesByContactId.TryGetValue(contactId, out int remainingFrames))
        {
            if(remainingFrames > 0)
            {
                _pendingSettleFramesByContactId[contactId] = remainingFrames - 1;
                return;
            }

            _pendingSettleFramesByContactId.Remove(contactId);
            OnPlaced(context);
            Activate(context);

            return;
        }

        if(IsBehaviorActive(context.ActiveContact) == false)
        {
            // Contact is ongoing, but behavior needs to be activated
            Activate(context);
        }
        
        OnUpdate(context);
    }

    private void Deactivate(PieceBehaviorContext context)
    {
        _pendingSettleFramesByContactId.Remove(context.ActiveContact.contactId);
        
        if(IsBehaviorActive(context.ActiveContact) == false) { return; }

        OnDeactivate(context);
        SetBehaviorActive(context.ActiveContact, false);
    }
}
}
