using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
    [CreateAssetMenu(menuName = "Piece System/" + nameof(PieceOrientation))]
    public class PieceOrientation : ScriptableObject
    {
        public float Value;

        public float ValueDegrees { get => Value * Mathf.Rad2Deg; set => Value = value * Mathf.Deg2Rad; }
    }
}
