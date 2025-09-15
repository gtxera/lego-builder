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

    public event Action<Color> ColorChanged = delegate { }; 

    public PieceColor GetSelectedColorFor(int index) => _colors[index];

    public void SetColor(Color color)
    {
        _colors[0] = new SimpleColor(color);
        ColorChanged(_colors[0].Color);
    }
}