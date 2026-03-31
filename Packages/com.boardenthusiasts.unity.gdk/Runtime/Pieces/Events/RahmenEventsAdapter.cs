using Rahmen.Events;

namespace BoardGDK.Pieces.Events
{
/// <summary>
/// Adapts piece-related C# events to expose them through the <see cref="IEventSystem"/>.
/// </summary>
public class RahmenEventsAdapter
{
    private readonly IEventSystem _eventSystem;
    private readonly IPieceSystem _pieceSystem;
    
    public RahmenEventsAdapter(IEventSystem eventSystem, IPieceSystem pieceSystem)
    {
        _eventSystem = eventSystem;
        _pieceSystem = pieceSystem;
        
        _pieceSystem.PieceSetChanged += OnPieceSetChanged;
        _pieceSystem.PiecePlaced += OnPiecePlaced;
        _pieceSystem.PiecePickedUp += OnPiecePickedUp;
    }

    private void OnPieceSetChanged(object sender, IPieceSetDefinitionEvent e)
    {
        _eventSystem.Raise(new PieceSetChanged
        {
            Definition = e.Definition
        }, sender);
    }

    private void OnPiecePlaced(object sender, IPieceEvent e)
    {
        _eventSystem.Raise(new PiecePlaced
        {
            VirtualPiece = e.VirtualPiece
        }, sender);
    }

    private void OnPiecePickedUp(object sender, IPieceEvent e)
    {
        _eventSystem.Raise(new PiecePickedUp
        {
            VirtualPiece = e.VirtualPiece
        }, sender);
    }
}
}
