using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;

using Zenject;

using Object = UnityEngine.Object;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> which instantiates prefab(s) at a Piece's position upon activation of the behavior, and
/// destroys the instantiated prefab(s) upon deactivation of the behavior.
/// </summary>
/// <remarks>
/// <para>
/// The prefabs spawned will be children of the associated <see cref="IVirtualPiece"/>, and so will be moved along with
/// the piece as it moves and rotates on Board, but will maintain their own local transform relative to the piece.
/// </para>
/// <para>
/// If you wish to spawn objects that are not tied to the lifecycle of the piece and its location, use the
/// <see cref="Spawn"/> behavior instead.
/// </para>
/// </remarks>
[Serializable]
public class SpawnSynced : PieceBehavior
{
    [Tooltip("The prefabs to spawn.")]
    [SerializeField]
    private GameObject[] m_prefabsToSpawn = Array.Empty<GameObject>();

    private IInstantiator _instantiator;
    private IRahmenLogger _logger;
    private const string _prefabInstancesKey = "PrefabInstances";

    /// <inheritdoc />
    public override void Place(PieceBehaviorContext context)
    {
        List<GameObject> prefabInstances = UnityEngine.Pool.ListPool<GameObject>.Get();
        for(int i = 0; i < m_prefabsToSpawn.Length; ++i)
        {
            GameObject prefab = m_prefabsToSpawn[i];
            
            if(prefab == null)
            {
                _logger.Warning()?.Log($"Prefab at position <{i}> is null. You should remove this entry.");
                continue;
            }
            
            _logger.Debug()?.Log($"Instantiating prefab <{prefab.name}> as child of <{context.VirtualPiece.Name}>.");
            GameObject prefabInstance = _instantiator.InstantiatePrefab(prefab, context.VirtualPiece.AnchorTransform);
            context.VirtualPiece.AddDigitalPiece(prefabInstance);
            prefabInstance.SetActive(false);
            prefabInstances.Add(prefabInstance);
        }
        
        context.State.SetValue(_prefabInstancesKey, prefabInstances);
    }

    /// <inheritdoc />
    public override void Activate(PieceBehaviorContext context)
    {
        if(context.State.TryGetValue(_prefabInstancesKey, out List<GameObject> prefabInstances) == false) { return; }
        for(int i = 0; i < prefabInstances.Count; ++i)
        {
            prefabInstances[i].SetActive(true);
        }
    }

    /// <inheritdoc />
    public override void Update(PieceBehaviorContext context) { }

    /// <inheritdoc />
    public override void Deactivate(PieceBehaviorContext context)
    {
        if(context.State.TryGetValue(_prefabInstancesKey, out List<GameObject> prefabInstances) == false) { return; }
        for(int i = 0; i < prefabInstances.Count; ++i)
        {
            prefabInstances[i].SetActive(false);
        }
    }

    /// <inheritdoc />
    public override void PickUp(PieceBehaviorContext context)
    {
        if(context.State.TryGetValue(_prefabInstancesKey, out List<GameObject> prefabInstances) == false) { return; }
        foreach(GameObject prefabInstance in prefabInstances)
        {
            if(prefabInstance == null)
            {
                _logger.Trace()?.Log($"Tracked prefab instance <{prefabInstance.name}> was already destroyed by other means.");
                
                continue;
            }
            
            _logger.Debug()?.Log($"Destroying prefab instance <{prefabInstance.name}>.");

            context.VirtualPiece.RemoveDigitalPiece(prefabInstance);
            Object.Destroy(prefabInstance);
        }

        UnityEngine.Pool.ListPool<GameObject>.Release(prefabInstances);
        context.State.SetValue(_prefabInstancesKey, default(List<GameObject>));
    }

    [Inject]
    private void Injection([NotNull] ILoggerFactory loggerFactory, [NotNull] IInstantiator instantiator)
    {
        _logger = loggerFactory.Get<LogChannels.PieceSystem>(this);
        _instantiator = instantiator;
    }
}
}
