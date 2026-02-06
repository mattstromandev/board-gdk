using System;

using Board.Input;

using BoardGDK.Extensions.Unity;

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
        return me.GetWorldPosition(Camera.main);
    }
    
    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using <see cref="Camera.main"/> and the specified target Z world coordinate.
    /// </summary>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <returns>The world position of this contact at the referenced Z world position.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Vector3 worldUpAxis)
    {
        return me.GetWorldPosition(Camera.main, worldUpAxis);
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
        return me.GetWorldPosition(Camera.main, worldUpAxis, surfaceLayers);
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
        return me.GetWorldPosition(Camera.main, worldUpAxis, surfaceLayers, referenceWorldPosition);
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the specified camera.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> to use for reference.</param>
    /// <returns>The world position of this contact from the perspective of the given camera.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Camera camera)
    {
        return camera.ScreenToWorldPoint(new Vector3(me.screenPosition.x, me.screenPosition.y, camera.nearClipPlane));
    }

    /// <summary>
    /// Retrieve the world position of this <see cref="BoardContact"/>, using the specified camera and providing a world up axis.
    /// </summary>
    /// <param name="camera">The <see cref="Camera"/> to use for reference.</param>
    /// <param name="worldUpAxis">The vector that represents the world up axis.</param>
    /// <returns>The world position of this contact from the perspective of the given camera.</returns>
    public static Vector3 GetWorldPosition(this BoardContact me, Camera camera, Vector3 worldUpAxis)
    {
        // Use the center screen point plane at the camera's near clip plane distance as the world point of reference
        Vector3 centerWorldPoint = camera.ScreenToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
        
        return me.GetWorldPosition(camera, worldUpAxis, centerWorldPoint);
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
        Vector3 screenPosition = me.screenPosition;
        Ray screenRay = camera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0f));
        
        Vector3 resolvedWorldUpAxis = worldUpAxis.ResolveWorldUp();
        Plane movementPlane = new(resolvedWorldUpAxis, referenceWorldPosition);

        if(movementPlane.Raycast(screenRay, out float enter) == false) { return Vector3.zero; }

        return screenRay.GetPoint(enter);
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
        // Use the center screen point plane at the camera's near clip plane distance as the world point of reference
        Vector3 centerWorldPoint = camera.ScreenToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
        
        return me.GetWorldPosition(camera, worldUpAxis, surfaceLayers, centerWorldPoint);
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
        if(camera == null)
        {
            throw new ArgumentNullException(nameof(camera));
        }
        
        Vector3 screenPosition = me.screenPosition;
        Ray screenRay = camera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0f));

        float raycastDistance = camera.farClipPlane;
        if(raycastDistance <= 0f) { raycastDistance = Mathf.Infinity; }

        if(Physics.Raycast(screenRay, out RaycastHit hit, raycastDistance, surfaceLayers))
        {
            return hit.point;
        }

        // Fall back to the non-physics approach
        return me.GetWorldPosition(camera, worldUpAxis, referenceWorldPosition);
    }
}
}
