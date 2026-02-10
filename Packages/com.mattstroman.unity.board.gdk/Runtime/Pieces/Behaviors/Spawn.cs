using System;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> which spawns prefabs from a Piece's position at specified lifecycle states, and at a
/// particular frequency during the behavior's update, if set.
/// </summary>
/// <remarks>
/// <para>
/// The prefabs spawned match the rotation at the time of spawning, but will be completely detached from this
/// <see cref="PieceBehavior"/>'s lifecycle and will not be automatically destroyed at any point. It is the responsibility
/// of the prefab instance itself, or some external system, to manage the lifecycle of the spawned prefab instance(s).
/// </para>
/// <para>
/// If you wish to spawn objects that are tied to the lifecycle of the piece and its location, use the
/// <see cref="SpawnSynced"/> behavior instead.
/// </para>
/// </remarks>
[Serializable]
public class Spawn : PieceBehavior
{
    [Tooltip("The prefabs to spawn.")]
    [SerializeField]
    private GameObject[] m_prefabsToSpawn = Array.Empty<GameObject>();
    
    [Tooltip("The offset from the Piece's center (screen space) at which to spawn the prefab.")]
    [SerializeField]
    private Vector2 m_spawnOffsetFromCenter;
    
    [Tooltip("The frequency, in seconds, at which to spawn the prefab.")]
    [SerializeField]
    private float m_spawnFrequency = 1f;

    [Tooltip("The behavior lifecycle mask indicating in what states to spawn the prefabs.")]
    [SerializeField]
    private PieceBehaviorLifecycleMask m_spawnOn = PieceBehaviorLifecycleMask.Activate;

    private IInstantiator _instantiator;
    private IRahmenLogger _logger;
    private const string _lastSpawnTimeKey = "LastSpawnTime";

    /// <inheritdoc />
    public override void Place(PieceBehaviorContext context)
    {
        context.State.SetValue(_lastSpawnTimeKey, 0f);
        if(m_spawnOn.HasFlag(PieceBehaviorLifecycleMask.Place)) { Instantiate(context); }
    }

    /// <inheritdoc />
    public override void Activate(PieceBehaviorContext context)
    {
        if(m_spawnOn.HasFlag(PieceBehaviorLifecycleMask.Activate)) { Instantiate(context); }
    }

    /// <inheritdoc />
    public override void Update(PieceBehaviorContext context)
    {
        if(context.State.TryGetValue(_lastSpawnTimeKey, out float lastSpawnTime) == false) { return; }
        
        float elapsedTime = Time.time - lastSpawnTime;

        if(elapsedTime < m_spawnFrequency) { return; }

        if(m_spawnOn.HasFlag(PieceBehaviorLifecycleMask.Update))
        {
            Instantiate(context);
            context.State.SetValue(_lastSpawnTimeKey, Time.time);
        }
    }

    /// <inheritdoc />
    public override void Deactivate(PieceBehaviorContext context)
    {
        if(m_spawnOn.HasFlag(PieceBehaviorLifecycleMask.Deactivate)) { Instantiate(context); }
    }

    /// <inheritdoc />
    public override void PickUp(PieceBehaviorContext context)
    {
        if(m_spawnOn.HasFlag(PieceBehaviorLifecycleMask.PickUp)) { Instantiate(context); }
    }

    [Inject]
    private void Injection([NotNull] IInstantiator instantiator, [NotNull] ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.Get<LogChannels.PieceBehaviorSystem>(this);
        _instantiator = instantiator;
    }

    private void Instantiate(PieceBehaviorContext context)
    {
        Camera mainCamera = Camera.main;
        Vector3 screenPosition = context.ContactState.screenPosition + m_spawnOffsetFromCenter;

        if(context.VirtualPiece != null)
        {
            screenPosition.z = mainCamera.WorldToScreenPoint(context.VirtualPiece.AnchorTransform.position).z;
        }
        else { screenPosition.z = mainCamera.nearClipPlane; }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        for(int i = 0; i < m_prefabsToSpawn.Length; ++i)
        {
            GameObject prefab = m_prefabsToSpawn[i];
            
            if(prefab == null)
            {
                _logger.Warning()?.Log($"Prefab at position <{i}> is null. You should remove this entry.");
                continue;
            }
            
            _logger.Debug()?.Log($"Spawning prefab <{prefab.name}> at world position <{worldPosition}>.");
            _instantiator.InstantiatePrefab(prefab, worldPosition, context.VirtualPiece.AnchorTransform.rotation, null);
        }
    }
}
}
