using System;

using Board.Input;

using BoardGDK.Extensions.Board;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> which syncs the position of a digital piece to that of the physical Piece on Board.
/// </summary>
[Serializable]
public class SyncPosition : PieceBehavior
{
    [Tooltip("The axis treated as world up when determining the movement plane.")]
    [SerializeField]
    private Vector3 m_worldUpAxis = Vector3.up;

    [Tooltip("Layers to raycast against when resolving the surface position.")]
    [SerializeField]
    private LayerMask m_surfaceLayers = ~0;

    /// <inheritdoc />
    protected override void OnActivate(PieceBehaviorContext context) { SetPosition(context); }

    /// <inheritdoc />
    protected override void OnUpdate(PieceBehaviorContext context)
    {
        if(context.ActiveContact.phase != BoardContactPhase.Moved) { return; }

        SetPosition(context);
    }

    protected override void OnDeactivate(PieceBehaviorContext context) { }

    private void SetPosition(PieceBehaviorContext context)
    {
        Camera mainCamera = Camera.main;
        if(mainCamera == null) { return; }

        Transform anchorTransform = context.VirtualPiece.AnchorTransform;
        Vector3 currentWorldPosition = anchorTransform.position;
        Vector3 targetWorldPosition = context.ActiveContact.GetWorldPosition(mainCamera, m_worldUpAxis, currentWorldPosition);
        // TODO: need to work on this surface resolution logic because it seems busted. If the surface layers is set to
        // Nothing (0), it should skip the raycast, but currently it still does it.
        // targetWorldPosition = ResolveSurfacePosition(targetWorldPosition, digitalTransform, mainCamera);

        anchorTransform.position = targetWorldPosition;
    }

    private Vector3 ResolveSurfacePosition(Vector3 baseWorldPosition, Transform digitalTransform, Camera mainCamera)
    {
        if(m_surfaceLayers.value == 0) { return baseWorldPosition; }

        Vector3 resolvedWorldUpAxis = ResolveWorldUpAxis();
        float raycastDistance = mainCamera.farClipPlane;
        if(raycastDistance <= 0f || float.IsInfinity(raycastDistance)) { raycastDistance = 1000f; }

        Vector3 rayOrigin = baseWorldPosition + resolvedWorldUpAxis * raycastDistance;
        float rayDistance = raycastDistance * 2f;
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, -resolvedWorldUpAxis, rayDistance, m_surfaceLayers);

        if(hits.Length == 0) { return baseWorldPosition; }

        bool hasHit = false;
        float closestDistance = float.PositiveInfinity;
        RaycastHit closestHit = default;

        for(int i = 0; i < hits.Length; ++i)
        {
            RaycastHit hit = hits[i];

            if(hit.collider == null) { continue; }
            if(digitalTransform != null && hit.collider.transform.IsChildOf(digitalTransform)) { continue; }

            if(hit.distance >= closestDistance) { continue; }

            closestDistance = hit.distance;
            closestHit = hit;
            hasHit = true;
        }

        if(hasHit == false) { return baseWorldPosition; }

        float axisOffset = Vector3.Dot(closestHit.point - baseWorldPosition, resolvedWorldUpAxis);
        return baseWorldPosition + resolvedWorldUpAxis * axisOffset;
    }

    private Vector3 ResolveWorldUpAxis()
    {
        if(m_worldUpAxis.sqrMagnitude <= Mathf.Epsilon) { return Vector3.up; }

        return m_worldUpAxis.normalized;
    }
}
}
