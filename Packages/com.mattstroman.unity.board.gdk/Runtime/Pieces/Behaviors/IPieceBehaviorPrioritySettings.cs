using System.Collections.Generic;

using Board.Input;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Interface for priority settings intended to allow configuration of execution order for <see cref="IPieceBehavior"/>s
/// per behavior type and <see cref="BoardContactPhase"/>.
/// </summary>
public interface IPieceBehaviorPrioritySettings
{
    /// <summary>
    /// The set of <see cref="PieceBehaviorPrioritySetting"/>s defining execution priorities for <see cref="IPieceBehavior"/>s.
    /// </summary>
    public IReadOnlyCollection<PieceBehaviorPrioritySetting> Priorities { get; }
}
}
