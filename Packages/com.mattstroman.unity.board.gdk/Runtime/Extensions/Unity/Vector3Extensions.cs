using UnityEngine;

namespace BoardGDK.Extensions.Unity
{
/// <summary>
/// Extension methods for the <see cref="UnityEngine.Vector3"/> struct.
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    /// Resolve this vector as a world up direction, defaulting to <see cref="Vector3.up"/> if the vector is zero.
    /// </summary>
    /// <returns>This vector as a world up direction, if applicable, or <see cref="Vector3.up"/>.</returns>
    public static Vector3 ResolveWorldUp(this Vector3 me)
    {
        return me.sqrMagnitude <= Mathf.Epsilon ? Vector3.up : me.normalized;
    }
}
}
