using UnityEngine;

public interface IEditablePieceTarget
{
    bool CanRotate { get; }
    Piece ReferencePiece { get; }

    void BeginMove(Piece referencePiece);
    void UpdateMove(Vector3 targetPosition);
    ICommand EndMove();
    void RotateClockwise();
    void RotateCounterClockwise();
    ICommand Paint(PieceColor color);
    ICommand Remove();
}
