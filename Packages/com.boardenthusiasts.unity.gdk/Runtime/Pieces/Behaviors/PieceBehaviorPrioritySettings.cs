using System.Collections.Generic;

using Board.Input;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Priority settings intended to allow configuration of execution order for <see cref="IPieceBehavior"/>s per behavior
/// type and <see cref="BoardContactPhase"/>.
/// </summary>
[CreateAssetMenu(menuName = "Prototype/" + nameof(PieceBehaviorPrioritySettings))]
public class PieceBehaviorPrioritySettings : ScriptableObject, IPieceBehaviorPrioritySettings
{
    [SerializeField]
    [Tooltip("The set of priority settings defining execution priorities for piece behaviors.")]
    private PieceBehaviorPrioritySetting[] m_priorities;

    /// <inheritdoc />
    public IReadOnlyCollection<PieceBehaviorPrioritySetting> Priorities => m_priorities;
}
}
