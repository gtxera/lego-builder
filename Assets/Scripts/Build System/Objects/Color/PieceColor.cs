using UnityEngine;

public abstract class PieceColor
{
    public Color Color
    {
        get => GetColor();

        set
        {
            SetColor(value);
            NamedColor = new NamedColor(value);
        }
    }

    public NamedColor NamedColor { get; private set; }

    protected abstract Color GetColor();
    protected abstract void SetColor(Color color);
    
    public abstract bool IsEqual(PieceColor pieceColor);
}
