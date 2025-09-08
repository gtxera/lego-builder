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

    public override Color Color
    {
        get => _color;
        set
        {
            _color = value;
            ColorChanged(_color);
        }
    }

    public override bool Equals(object other) => other is SwatchColor swatchColor && swatchColor == this;
}
