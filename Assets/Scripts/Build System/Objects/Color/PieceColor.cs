using UnityEngine;

public abstract class PieceColor
{
    public abstract Color Color { get; set; }

    public abstract override bool Equals(object other);
}
