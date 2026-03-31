using System;

using Board.Input;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Configuration for priorities of a single <see cref="IPieceBehavior"/> based on its type and <see cref="BoardContactPhase"/>.
/// </summary>
[Serializable]
public struct PieceBehaviorPrioritySetting
{
    [Tooltip("The type name of the piece behavior to configure the priority for.")]
    public string BehaviorTypeName;
    
    [Tooltip("The contact phase this priority applies to.")]
    public BoardContactPhase Phase;
    
    [Tooltip("The priority for this behavior type and contact phase. Lower numbers execute first.")]
    public int Priority;
}
}
