using System;
using System.Linq;

using Board.Input;

using BoardGDK.Touch;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// 
/// </summary>
[Serializable]
public class TouchProcessor : PieceBehavior
{
    [Tooltip("Range from the center of the piece that accepts touches for processing.")]
    [SerializeField]
    private float m_touchAcceptanceRange;

    [SerializeField] private LayerMask m_hitLayerMask;

    private Camera _mainCamera;
    private LineRenderer _lineRenderer;

    protected override void OnActivate(PieceBehaviorContext context) { }

    /// <inheritdoc />
    protected override void OnUpdate(PieceBehaviorContext context)
    {
        foreach(BoardContact activeTouch in BoardInput.GetActiveContacts(BoardContactType.Finger))
        {
            if(activeTouch.phase == BoardContactPhase.Ended) { continue; }

            Vector3 worldStartPos = _mainCamera.ScreenToWorldPoint(
                new Vector3(
                    activeTouch.screenPosition.x
                  , activeTouch.screenPosition.y, _mainCamera.nearClipPlane
                )
            );
            _lineRenderer.SetPosition(0, worldStartPos);
            _lineRenderer.SetPosition(1, worldStartPos + _mainCamera.transform.forward * 10000);

            float touchDistanceFromPieceCenter
                = Vector2.Distance(context.ActiveContact.screenPosition, activeTouch.screenPosition);

            if(touchDistanceFromPieceCenter > m_touchAcceptanceRange)
            {
                Debug.Log(
                    $"{nameof(TouchProcessor)}: ignoring touch at <{activeTouch.screenPosition}> as it is not within range <{m_touchAcceptanceRange}>. Distance was <{touchDistanceFromPieceCenter}>"
                );

                continue;
            }

            Camera cameraToRayFrom = _mainCamera;

            // TODO: look at what this commented code was trying to achieve
            // if(piece.DigitalPiece != null)
            // {
            //     Camera embeddedCamera = piece.DigitalPiece.GetComponentInChildren<Camera>();
            //
            //     if(embeddedCamera != null)
            //     {
            //         cameraToRayFrom = embeddedCamera;
            //     }
            // }
            //
            // Debug.Log($"{nameof(TouchProcessor)}: processing touch at <{activeTouch.screenPosition}> with camera <{cameraToRayFrom.name}>.");

            Ray touchRay = cameraToRayFrom.ScreenPointToRay(activeTouch.screenPosition);

            if(Physics.Raycast(touchRay, out RaycastHit hitInfo, float.PositiveInfinity, m_hitLayerMask))
            {
                ITouchable[] touchables = hitInfo.collider.GetComponentsInChildren<Component>().OfType<ITouchable>()
                    .ToArray();

                foreach(ITouchable touchable in touchables)
                {
                    Debug.Log($"{nameof(TouchProcessor)}: applying touch to <{hitInfo.collider.name}>");
                    touchable.OnTouch();
                }
            }
        }
    }

    protected override void OnDeactivate(PieceBehaviorContext context) {}

    [Inject]
    private void Injection()
    {
        _mainCamera = Camera.main;

        // TODO: keep or remove?
        // GameObject newGO = new("DebugRayCaster");
        // _lineRenderer = newGO.AddComponent<LineRenderer>();
        // _lineRenderer.startWidth = 5;
        // _lineRenderer.endWidth = 5;
        // _lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material for the line
        // _lineRenderer.startColor = Color.red;
        // _lineRenderer.endColor = Color.red;
        // _lineRenderer.positionCount = 2;
    }
}
}
