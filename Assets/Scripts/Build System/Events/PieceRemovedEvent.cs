public class PieceRemovedEvent : IEvent
{
    public PieceRemovedEvent(Piece piece)
    {
        Piece = piece;
    }

    public Piece Piece { get; }

}
