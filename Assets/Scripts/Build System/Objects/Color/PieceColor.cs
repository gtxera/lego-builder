using System;
using UnityEngine;

[Serializable]
public abstract class PieceColor
{
    private NamedColor _namedColor;
    
    [SerializeField]
    private bool _transparent;

    public Color Color
    {
        get => GetColor();

        set
        {
            SetColor(value);
            _namedColor = new NamedColor(value, _transparent);
        }
    }

    public bool Transparent
    {
        get => _transparent;
        set
        {
            _transparent = value;
            _namedColor = new NamedColor(Color, _transparent);
        }
    }

    public NamedColor NamedColor => _namedColor ??= new NamedColor(GetColor(), _transparent);

    protected abstract Color GetColor();
    protected abstract void SetColor(Color color);
    
    public abstract bool IsEqual(PieceColor pieceColor);
}
