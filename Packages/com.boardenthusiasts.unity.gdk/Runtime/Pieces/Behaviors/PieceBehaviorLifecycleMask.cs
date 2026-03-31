using System;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// Flags indicating during which states of an <see cref="IPieceBehavior"/>'s lifecycle an effect it applies should happen.
/// </summary>
[Flags]
public enum PieceBehaviorLifecycleMask : short
{
    None = 0,
    Place = 1 << 0,
    Activate = 1 << 1,
    Update = 1 << 2,
    Deactivate = 1 << 3,
    PickUp = 1 << 4,
    Injection = 1 << 5,
    AllButInjection = Place | Activate | Update | Deactivate | PickUp,
    All = AllButInjection | Injection
}
}
