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
    private GameObject _digitalPieceInstance;

    /// <inheritdoc />
    protected override void OnActivate(PieceBehaviorContext context)
    {
        _digitalPieceInstance = _instantiator.InstantiatePrefab(m_prefab, context.VirtualPiece.AnchorTransform);
        context.VirtualPiece.AddDigitalPiece(_digitalPieceInstance);
    }

    protected override void OnUpdate(PieceBehaviorContext context) { }

    /// <inheritdoc />
    protected override void OnDeactivate(PieceBehaviorContext context)
    {
        if(_digitalPieceInstance == null) { return; }
        
        context.VirtualPiece.RemoveDigitalPiece(_digitalPieceInstance);
        Object.Destroy(_digitalPieceInstance);
    }

    [Inject]
    private void Injection(IInstantiator instantiator) { _instantiator = instantiator; }
}
}
