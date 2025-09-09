using UnityEngine;

public class SimpleColor : PieceColor
{
    public SimpleColor(Color color)
    {
        Color = color;
    }

    public sealed override Color Color { get; set; }
    
    public override bool IsEqual(PieceColor pieceColor) => pieceColor is SimpleColor simpleColor && simpleColor.Color == Color;
}
