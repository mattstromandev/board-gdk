using System;

using Board.Input;

using UnityEngine;

namespace BoardGDK.Pieces.Attributes
{
/// <summary>
/// <see cref="PropertyAttribute"/> that can be applied to serialized fields storing <see cref="BoardContactType.Glyph"/>
/// IDs to make them display the configured piece name in the Unity Inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PieceNameAttribute : PropertyAttribute { }
}
