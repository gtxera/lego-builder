using System;
using UnityEngine;

public class SwatchColor : PieceColor
{
    private Color _color;

    public SwatchColor(Color color)
    {
        _color = color;
    }

    public event Action<Color> ColorChanged = delegate { };

    protected override Color GetColor() => _color;

    protected override void SetColor(Color color)
    {
        _color = color;
        ColorChanged(_color);
    }

    public override bool IsEqual(PieceColor pieceColor) => pieceColor is SwatchColor swatchColor && swatchColor == this;
}
