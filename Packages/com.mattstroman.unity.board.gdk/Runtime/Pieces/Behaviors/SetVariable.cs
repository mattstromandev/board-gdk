using System;

using BoardGDK.Data;

using UnityEngine;
using UnityEngine.Serialization;

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
    
    [FormerlySerializedAs("ActiveValue")]
    [Tooltip("The value to set when activated.")]
    [SerializeField]
    private T m_activeValue;
    
    [FormerlySerializedAs("InactiveValue")]
    [Tooltip("The value to set when deactivated.")]
    [SerializeField]
    private T m_inactiveValue;

    [Inject]
    private void Injection() { m_variable.Value = m_inactiveValue; }

    /// <inheritdoc />
    protected override void OnActivate(PieceBehaviorContext context) { m_variable.Value = m_activeValue; }

    protected override void OnUpdate(PieceBehaviorContext context) { }

    /// <inheritdoc />
    protected override void OnDeactivate(PieceBehaviorContext context) { m_variable.Value = m_inactiveValue; }
}
}
