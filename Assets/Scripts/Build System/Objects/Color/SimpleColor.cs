using System;
using UnityEngine;

[Serializable]
public class SimpleColor : PieceColor
{
    [SerializeField]
    private Color _color;
    
    public SimpleColor(Color color, bool transparent = false)
    {
        Color = color;
        Transparent = transparent;
    }

    protected override Color GetColor() => _color;

    protected override void SetColor(Color color) => _color = color;

    public override bool IsEqual(PieceColor pieceColor) => pieceColor is SimpleColor simpleColor && simpleColor.Color == Color;
}
