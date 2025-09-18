using UnityEngine;

public class PieceMovedEvent : IEvent
{
    public PieceMovedEvent(Piece piece, Vector3 previousPosition, Vector3 currentPosition)
    {
        Piece = piece;
        PreviousPosition = previousPosition;
        CurrentPosition = currentPosition;
    }

    public Piece Piece { get; }
    public Vector3 PreviousPosition { get; }
    public Vector3 CurrentPosition { get; }
}