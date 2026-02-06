using System;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Definition for a common set of <see cref="PieceBehavior"/>s that can be applied to multiple pieces.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece Behavior Set")]
public class PieceBehaviorSet : ScriptableObject
{
    /// <summary>
    /// The set of <see cref="PieceBehavior"/>s that will be applied together.
    /// </summary>
    [Tooltip("The set of PieceBehaviors that will be applied together.")]
    public PieceBehavior[] Behaviors = Array.Empty<PieceBehavior>();
}
}
