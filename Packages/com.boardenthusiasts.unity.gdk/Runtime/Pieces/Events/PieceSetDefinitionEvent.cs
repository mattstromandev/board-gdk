using System;

namespace BoardGDK.Pieces.Events
{
/// <summary>
/// Event involving an <see cref="IPieceSetDefinitionEvent"/> (e.g. definition is changed).
/// </summary>
public class PieceSetDefinitionEvent : EventArgs, IPieceSetDefinitionEvent
{
    public virtual IPieceSetDefinition Definition { get; set; }
}
}
