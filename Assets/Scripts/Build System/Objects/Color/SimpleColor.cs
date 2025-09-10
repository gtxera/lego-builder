using UnityEngine;

public class SimpleColor : PieceColor
{
    private Color _color;
    
    public SimpleColor(Color color)
    {
        Color = color;
    }

    protected override Color GetColor() => _color;

    protected override void SetColor(Color color) => _color = color;

    public override bool IsEqual(PieceColor pieceColor) => pieceColor is SimpleColor simpleColor && simpleColor.Color == Color;
}
