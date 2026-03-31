using System;

using Rahmen.Events;

using UnityEngine;

using EventSource = Rahmen.Events.EditorInterface.EventSource;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> which raises configured <see cref="IRahmenEvent"/>s for each of a piece's lifecycle
/// events (placement, activation, update, deactivation, and pickup).
/// </summary>
[Serializable]
public class RaiseEvents : PieceBehavior
{
    [SerializeField] private EventSource[] m_placed = Array.Empty<EventSource>();
    [SerializeField] private EventSource[] m_activated = Array.Empty<EventSource>();
    [SerializeField] private EventSource[] m_update = Array.Empty<EventSource>();
    [SerializeField] private EventSource[] m_deactivated = Array.Empty<EventSource>();
    [SerializeField] private EventSource[] m_pickedUp = Array.Empty<EventSource>();
    
    public override void Place(PieceBehaviorContext context)
    {
        throw new NotImplementedException();
    }

    public override void Activate(PieceBehaviorContext context)
    {
        throw new NotImplementedException();
    }

    public override void Update(PieceBehaviorContext context)
    {
        throw new NotImplementedException();
    }

    public override void Deactivate(PieceBehaviorContext context)
    {
        throw new NotImplementedException();
    }

    public override void PickUp(PieceBehaviorContext context)
    {
        throw new NotImplementedException();
    }
}
}
