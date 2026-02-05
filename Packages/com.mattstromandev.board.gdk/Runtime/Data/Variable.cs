using System;

using UnityEngine;

namespace BoardGDK.Data
{
public abstract class Variable<T> : ScriptableObject
{
    [SerializeField] protected T m_initialValue;
    [SerializeField] protected T m_value;

    public T Value
    {
        get => m_value;
        set
        {
            if(value.Equals(m_value)) { return; }

            T previousValue = m_value;
            m_value = value;
            OnValueChanged?.Invoke(previousValue, m_value);
        }
    }

    public event Action<T, T> OnValueChanged;

    protected virtual void OnEnable() { m_value = m_initialValue; }
}
}
