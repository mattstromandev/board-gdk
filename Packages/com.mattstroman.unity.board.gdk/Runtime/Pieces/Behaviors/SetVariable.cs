using System;

using AYellowpaper.SerializedCollections;

using BoardGDK.Data;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Base for a <see cref="PieceBehavior"/> which sets a <see cref="Variable{T}"/> to different values.
/// </summary>
/// <typeparam name="TV">The type of <see cref="Variable{T}"/> to be set.</typeparam>
/// <typeparam name="T">The type of data to set.</typeparam>
[Serializable]
public abstract class SetVariable<TV, T> : PieceBehavior where TV : Variable<T>
{
    [Tooltip("The variable to set.")]
    [SerializeField]
    private TV m_variable;

    [Tooltip("The values to set at particular stages of the behavior's lifecycle.")]
    [SerializeField]
    [SerializedDictionary("Behavior States", "Value")]
    private SerializedDictionary<PieceBehaviorLifecycleMask, T> m_values = new();

    private IRahmenLogger _logger;

    [Inject]
    private void Injection([NotNull] ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.Get<LogChannels.PieceBehaviorSystem>(this);
        IterateValues(PieceBehaviorLifecycleMask.Injection);
    }

    /// <inheritdoc />
    public override void Place(PieceBehaviorContext context) { IterateValues(PieceBehaviorLifecycleMask.Place); }

    /// <inheritdoc />
    public override void Activate(PieceBehaviorContext context) { IterateValues(PieceBehaviorLifecycleMask.Activate); }

    /// <inheritdoc />
    public override void Update(PieceBehaviorContext context) { IterateValues(PieceBehaviorLifecycleMask.Update); }

    /// <inheritdoc />
    public override void Deactivate(PieceBehaviorContext context) { IterateValues(PieceBehaviorLifecycleMask.Deactivate); }

    /// <inheritdoc />
    public override void PickUp(PieceBehaviorContext context) { IterateValues(PieceBehaviorLifecycleMask.PickUp); }
    
    private void IterateValues(PieceBehaviorLifecycleMask activeLifecycle)
    {
        foreach((PieceBehaviorLifecycleMask mask, T value) in m_values)
        {
            if(mask.HasFlag(activeLifecycle) == false)
            {
                _logger.Trace()?.Log($"Current lifecycle <{activeLifecycle}> is not included in the lifecycle mask <{mask}>; not setting variable.");

                continue;
            }
            
            SetVariableValue(value);
        }
    }

    private void SetVariableValue(T newValue)
    {
        if(m_variable == null)
        {
            _logger.Warning()?.Log($"No variable is assigned. Cannot set variable to <{newValue}>.");

            return;
        }
        
        _logger.Debug()?.Log($"Setting variable <{m_variable.name}> to <{newValue}>.");
        m_variable.Value = newValue;
    }
}
}
