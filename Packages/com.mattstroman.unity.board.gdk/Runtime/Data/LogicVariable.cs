using System;
using System.Linq;

using UnityEngine;

namespace BoardGDK.Data
{
[CreateAssetMenu(menuName = "Game/Data/Logic")]
public class LogicVariable : BoolVariable
{
    [SerializeField] private LogicCondition[] m_conditions;

    [Serializable]
    private struct LogicCondition
    {
        public BoolVariable Variable;
        public bool TargetValue;
    }

    /// <inheritdoc />
    protected override void OnEnable()
    {
        base.OnEnable();

        foreach(LogicCondition condition in m_conditions)
        {
            if(condition.Variable == null) { continue; }

            condition.Variable.OnValueChanged += OnConditionChanged;
        }
    }

    private void OnDisable()
    {
        foreach(LogicCondition condition in m_conditions)
        {
            if(condition.Variable == null) { continue; }

            condition.Variable.OnValueChanged -= OnConditionChanged;
        }
    }

    private void OnConditionChanged(bool oldValue, bool newValue) { Value = EvaluateConditions(); }

    private bool EvaluateConditions()
    {
        return m_conditions.All(condition => condition.Variable.Value == condition.TargetValue);
    }
}
}
