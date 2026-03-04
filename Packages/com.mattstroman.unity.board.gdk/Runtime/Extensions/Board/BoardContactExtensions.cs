using System;

using Board.Input;

using Rahmen.Extensions;

using UnityEngine;

namespace BoardGDK.Extensions.Board
{
/// <summary>
/// Extension methods for the <see cref="BoardContact"/> class.
/// </summary>
public static class BoardContactExtensions
{
    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using <see cref="Camera.main"/>.
    /// </summary>
    /// <returns>The world position of this contact.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me)
    {
        return me.screenPosition.GetWorldPosition();
    }
    
    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using <see cref="Camera.main"/> and the specified target Z world coordinate.
    /// </summary>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <returns>The world position of this contact at the referenced Z world position.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Vector3 worldUpAxis)
    {
        return me.screenPosition.GetWorldPosition(worldUpAxis);
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the main camera as reference, and
    /// a physics check for surface detection.
    /// </summary>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <param name="surfaceLayers">The surface layers to allow for physics hit detection.</param>
    /// <returns>
    /// <para>
    /// The world position of this contact, from the perspective of the given camera.
    /// </para>
    /// <para>
    /// A physics raycast is performed against the specified <paramref name="surfaceLayers"/> to determine the world position.
    /// If no surface is hit, the world position is resolved against a movement plane defined by the specified <paramref name="worldUpAxis"/>
    /// and a point on the plane at the center of the screen at the camera's near clip plane distance. If that fails for
    /// any reason, the origin (<see cref="Vector3.zero"/>) is returned.
    /// </para>
    /// </returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Vector3 worldUpAxis, LayerMask surfaceLayers)
    {
        return me.screenPosition.GetWorldPosition(worldUpAxis, surfaceLayers);
    }
    
    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the main camera as reference, and
    /// a physics check for surface detection.
    /// </summary>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <param name="surfaceLayers">The surface layers to allow for physics hit detection.</param>
    /// <param name="referenceWorldPosition">A position to use as a reference for the fallback movement plan.</param>
    /// <returns>
    /// <para>
    /// The world position of this contact, from the perspective of the given camera.
    /// </para>
    /// <para>
    /// A physics raycast is performed against the specified <paramref name="surfaceLayers"/> to determine the world position.
    /// If no surface is hit, the world position is resolved against a movement plane defined by the specified <paramref name="worldUpAxis"/>
    /// and a point on the plane at the center of the screen at the camera's near clip plane distance. If that fails for
    /// any reason, the origin (<see cref="Vector3.zero"/>) is returned.
    /// </para>
    /// </returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Vector3 worldUpAxis, LayerMask surfaceLayers, Vector3 referenceWorldPosition)
    {
        return me.screenPosition.GetWorldPosition(worldUpAxis, surfaceLayers, referenceWorldPosition);
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the specified camera.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> to use for reference.</param>
    /// <returns>The world position of this contact from the perspective of the given camera.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Camera camera)
    {
        return me.screenPosition.GetWorldPosition(camera);
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the specified camera and providing a world up axis.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> to use for reference.</param>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <returns>The world position of this contact from the perspective of the given camera.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Camera camera, Vector3 worldUpAxis)
    {
        return me.screenPosition.GetWorldPosition(camera, worldUpAxis);
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the specified camera and providing a world up axis.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> to use for reference.</param>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <param name="referenceWorldPosition">A position to use as a reference for the movement plan.</param>
    /// <returns>The world position of this contact from the perspective of the given camera and <paramref name="referenceWorldPosition"/>.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Camera camera, Vector3 worldUpAxis, Vector3 referenceWorldPosition)
    {
        return me.screenPosition.GetWorldPosition(camera, worldUpAxis, referenceWorldPosition);
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the specified camera and providing a world up axis.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> to use for reference.</param>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <param name="screenSpaceOffset">An offset, in screen space, to apply before calculating the world position.</param>
    /// <param name="referenceWorldPosition">A position to use as a reference for the movement plan.</param>
    /// <returns>The world position of this contact from the perspective of the given camera and <paramref name="referenceWorldPosition"/>.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Camera camera, Vector3 worldUpAxis, Vector2 screenSpaceOffset, Vector3 referenceWorldPosition)
    {
        return me.screenPosition.GetWorldPosition(camera, worldUpAxis, screenSpaceOffset, referenceWorldPosition);
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the specified camera as reference, and
    /// a physics check for surface detection.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> to use for reference.</param>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <param name="surfaceLayers">The surface layers to allow for physics hit detection.</param>
    /// <returns>
    /// <para>
    /// The world position of this contact, from the perspective of the given camera.
    /// </para>
    /// <para>
    /// A physics raycast is performed against the specified <paramref name="surfaceLayers"/> to determine the world position.
    /// If no surface is hit, the world position is resolved against a movement plane defined by the specified <paramref name="worldUpAxis"/>
    /// and a point on the plane at the center of the screen at the camera's near clip plane distance. If that fails for
    /// any reason, the origin (<see cref="Vector3.zero"/>) is returned.
    /// </para>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="camera"/> is null.</exception>
    public static Vector3 GetWorldPosition(this BoardContact me, Camera camera, Vector3 worldUpAxis, LayerMask surfaceLayers)
    {
        return me.screenPosition.GetWorldPosition(camera, worldUpAxis, surfaceLayers);
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the specified camera as reference, and
    /// a physics check for surface detection.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> to use for reference.</param>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <param name="surfaceLayers">The surface layers to allow for physics hit detection.</param>
    /// <param name="referenceWorldPosition">A position to use as a reference for the fallback movement plan.</param>
    /// <returns>
    /// <para>
    /// The world position of this contact, from the perspective of the given camera.
    /// </para>
    /// <para>
    /// A physics raycast is performed against the specified <paramref name="surfaceLayers"/> to determine the world position.
    /// If no surface is hit, the world position is resolved against a movement plane defined by the specified <paramref name="worldUpAxis"/>
    /// and a point on the plane at the center of the screen at the camera's near clip plane distance. If that fails for
    /// any reason, the origin (<see cref="Vector3.zero"/>) is returned.
    /// </para>
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="camera"/> is null.</exception>
    public static Vector3 GetWorldPosition(this BoardContact me, Camera camera, Vector3 worldUpAxis, LayerMask surfaceLayers, Vector3 referenceWorldPosition)
    {
        return me.screenPosition.GetWorldPosition(camera, worldUpAxis, surfaceLayers, referenceWorldPosition);
    }
}
}
