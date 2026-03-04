using Rahmen;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="RahmenBehavior"/> to use in tandem with the <see cref="Spawn"/> piece behavior. Any behavior that you
/// have spawned by a <see cref="Spawn"/> piece behavior has the opportunity to have access to the <see cref="IVirtualPiece"/>
/// which spawned it. If you want access to this, you should derive from <see cref="SpawnedBehavior"/> attached to the root of the prefab for the purpose of linking the
/// have a <see cref="SpawnedBehavior"/> component attached to the root of the prefab for the purpose of linking the
/// spawned object to the <see cref="IVirtualPiece"/> that spawned it, if you wish to have access to the piece's
/// information and state from the spawned prefab.
/// </summary>
public abstract class SpawnedBehavior : RahmenBehavior
{
    [SerializeField]
    [Tooltip("The virtual piece that spawned this prefab instance. This is automatically set by the Spawn behavior immediately after the prefab is instantiated, and can be used to retrieve active data about the piece which spawned it.")]
    private VirtualPiece m_virtualPiece;

    /// <summary>
    /// The <see cref="IVirtualPiece"/> that spawned this prefab instance. This is automatically set by the
    /// <see cref="Spawn"/> behavior when the prefab is instantiated, and is used to link the spawned prefab to the piece
    /// that spawned it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this <b>will not be available during Awake()</b> of the spawned prefab, as the <see cref="Spawn"/>
    /// behavior sets this value immediately after instantiating the prefab. If you need to do any initialization that
    /// requires access to the <see cref="IVirtualPiece"/>, you should do so in Start() or a later lifecycle method.
    /// </para>
    /// <para>
    /// <b>Warning:</b> as the spawned object's lifecycle is completely independent from the piece that spawned it,
    /// there is no guarantee that the piece will still be active or even exist when the spawned prefab accesses this
    /// property. Always check for null before use and ensure that your code can handle the case where the piece is no
    /// longer active or has been removed from Board.
    /// </para>
    /// </remarks>
    protected IVirtualPiece VirtualPiece => m_virtualPiece;

    [Inject]
    private void Injection(IVirtualPiece virtualPiece) { m_virtualPiece = (VirtualPiece) virtualPiece; }
}
}
