using System;

using BoardGDK.Data;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors.Conditions
{
// TODO: Move this to a separate package that connects BoardGDK with Unity Atoms
[Serializable]
public abstract class Logic<TV, T> : PieceBehaviorCondition where TV : Variable<T>
{
    [SerializeField] private TV m_variable;
    [SerializeField] private T m_targetValue;

    /// <inheritdoc />
    protected override bool DoEvaluation(PieceBehaviorContext context)
    {
        if(m_variable != null) { return m_variable.Value.Equals(m_targetValue); }

        return true;
    }
}
}
