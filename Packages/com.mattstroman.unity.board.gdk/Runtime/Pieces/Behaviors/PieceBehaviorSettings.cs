using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="IPieceBehaviorSettings"/> exposed as a ScriptableObject for configuration in the Unity Editor.
/// </summary>
[CreateAssetMenu(menuName = Menus.PiecesMenuRoot + "Piece Behavior Settings")]
public class PieceBehaviorSettings : ScriptableObject, IPieceBehaviorSettings
{
    /// <inheritdoc />
    [field: SerializeField]
    [field: Tooltip("The axis in world space that is considered \"up\" for the purposes of piece behavior calculations.")]
    public Vector3 WorldUpAxis { get; private set; } = Vector3.up;
}
}
