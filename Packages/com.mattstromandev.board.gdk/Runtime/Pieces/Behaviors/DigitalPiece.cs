using System;

using UnityEngine;

using Zenject;

using Object = UnityEngine.Object;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> which instantiates a digital representation of the Piece on Board.
/// </summary>
[Serializable]
public class DigitalPiece : PieceBehavior
{
    [Tooltip("The prefab to instantiate as the digital piece.")]
    [SerializeField]
    private GameObject m_prefab;

    private IInstantiator _instantiator;

    /// <inheritdoc />
    protected override void OnActivate(PieceBehaviorContext context)
    {
        if(context.VirtualPiece.DigitalPiece != null) { return; }

        context.VirtualPiece.AssignDigitalPiece(_instantiator.InstantiatePrefab(m_prefab));
    }

    protected override void OnUpdate(PieceBehaviorContext context) { }

    /// <inheritdoc />
    protected override void OnDeactivate(PieceBehaviorContext context)
    {
        if(context.VirtualPiece.DigitalPiece == null) { return; }

        Object.Destroy(context.VirtualPiece.DigitalPiece);
    }

    [Inject]
    private void Injection(IInstantiator instantiator) { _instantiator = instantiator; }
}
}
