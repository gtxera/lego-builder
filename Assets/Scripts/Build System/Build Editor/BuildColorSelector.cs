using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildColorSelector
{
    private readonly List<PieceColor> _colors = new();

    public BuildColorSelector()
    {
        _colors.Add(new SimpleColor(Color.white));
    }

    public event Action<PieceColor> ColorChanged = delegate { }; 

    public PieceColor GetSelectedColorFor(int index) => _colors[index];
    public PieceColor SelectedColor => GetSelectedColorFor(0);

    public void SetColor(Color color, bool transparent)
    {
        SetColor(new SimpleColor(color, transparent));
    }

    public void SetColor(PieceColor color)
    {
        _colors[0] = Clone(color);
        ColorChanged(_colors[0]);
    }

    public static PieceColor Clone(PieceColor color)
    {
        return new SimpleColor(color.Color, color.Transparent);
    }
}
