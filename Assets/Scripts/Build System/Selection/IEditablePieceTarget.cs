using UnityEngine;

public interface IEditablePieceTarget
{
    bool CanRotate { get; }
    Piece ReferencePiece { get; }

    void BeginMove(Piece referencePiece);
    bool TryGetMovePosition(Ray ray, out Vector3 targetPosition);
    void UpdateMove(Vector3 targetPosition);
    ICommand EndMove();
    void RotateClockwise();
    void RotateCounterClockwise();
    ICommand Paint(PieceColor color);
    ICommand Remove();
}
