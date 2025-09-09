using UnityEngine;

public abstract class PieceColor
{
    public abstract Color Color { get; set; }

    public abstract bool IsEqual(PieceColor pieceColor);
}
