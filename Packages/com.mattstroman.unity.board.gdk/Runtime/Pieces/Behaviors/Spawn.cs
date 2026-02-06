using System;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> which spawns a prefab from a Piece's position at a specified frequency.
/// </summary>
[Serializable]
public class Spawn : PieceBehavior
{
    [Tooltip("The prefab to spawn.")]
    [SerializeField]
    private GameObject m_prefabToSpawn;
    
    [Tooltip("The offset from the Piece's center at which to spawn the prefab.")]
    [SerializeField]
    private Vector3 m_spawnOffsetFromCenter;
    
    [Tooltip("The frequency, in seconds, at which to spawn the prefab.")]
    [SerializeField]
    private float m_spawnFrequency = 1f;
    
    [Tooltip("Whether to spawn the prefab immediately upon activation of the behavior.")]
    [SerializeField]
    private bool m_spawnOnActivate;
    
    [Tooltip("Whether to spawn the prefab at the set frequency during update of the behavior.")]
    [SerializeField]
    private bool m_spawnOnDuringUpdate = true;
    
    [Tooltip("Whether to spawn the prefab immediately upon deactivation of the behavior.")]
    [SerializeField]
    private bool m_spawnOnDeactivate;

    private float _lastSpawnTime;
    private IInstantiator _instantiator;

    /// <inheritdoc />
    protected override void OnActivate(PieceBehaviorContext context)
    {
        if(m_spawnOnActivate) { Instantiate(context); }

        _lastSpawnTime = Time.time;
    }

    /// <inheritdoc />
    protected override void OnUpdate(PieceBehaviorContext context)
    {
        float elapsedTime = Time.time - _lastSpawnTime;

        if(elapsedTime < m_spawnFrequency) { return; }

        if(m_spawnOnDuringUpdate) { Instantiate(context); }
        _lastSpawnTime = Time.time;
    }

    protected override void OnDeactivate(PieceBehaviorContext context)
    {
        if(m_spawnOnDeactivate) { Instantiate(context); }
    }
    
    [Inject]
    private void Injection(IInstantiator instantiator) { _instantiator = instantiator; }

    private void Instantiate(PieceBehaviorContext context)
    {
        Camera mainCamera = Camera.main;
        Vector3 screenPosition = context.ActiveContact.screenPosition;

        if(context.VirtualPiece.DigitalPieces != null)
        {
            screenPosition.z = mainCamera.WorldToScreenPoint(context.VirtualPiece.AnchorTransform.position).z;
        }
        else { screenPosition.z = mainCamera.nearClipPlane; }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition += m_spawnOffsetFromCenter;

        _instantiator.InstantiatePrefab(m_prefabToSpawn, worldPosition, Quaternion.identity, null);
    }
}
}
