using System;

using Rahmen.Events;

using UnityEngine;

namespace BoardGDK.Pieces.Events
{
/// <summary>
/// Base <see cref="IRahmenEvent"/> wrapping a <see cref="PieceSetDefinitionEvent"/>. Used to create specific events involving
/// piece set definitions that can be raised through the <see cref="IEventSystem"/>.
/// </summary>
[Serializable]
public abstract class PieceSetDefinitionRahmenEvent : PieceSetDefinitionEvent, IRahmenEvent
{
    [SerializeField]
    [Tooltip("The piece set definition involved in this event.")]
    private PieceSetDefinition m_definition;

    public override IPieceSetDefinition Definition
    {
        get => m_definition;
        set => m_definition = value as PieceSetDefinition;
    }
}
}
