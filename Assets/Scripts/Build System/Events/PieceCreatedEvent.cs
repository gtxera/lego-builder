using UnityEngine;

public class PieceCreatedEvent : IEvent
{
    public PieceCreatedEvent(Piece piece)
    {
        Piece = piece;
    }

    public Piece Piece { get; }
}
