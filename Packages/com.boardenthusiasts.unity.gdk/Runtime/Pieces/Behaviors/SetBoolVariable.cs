using System;

using BoardGDK.Data;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="SetVariable{TV,T}"/> for the <see cref="bool"/> type.
/// </summary>
[Serializable]
public class SetBoolVariable : SetVariable<BoolVariable, bool> { }
}
