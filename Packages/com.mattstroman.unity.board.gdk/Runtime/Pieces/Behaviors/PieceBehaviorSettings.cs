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
    public bool UsePieceSettling { get; private set; } = true;

    /// <inheritdoc />
    [field: SerializeField]
    public int PieceSettlingFrames { get; private set; } = 3;
}
}
