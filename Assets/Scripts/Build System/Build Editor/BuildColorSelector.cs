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

    public void SetColor(Color color, bool transparent)
    {
        _colors[0] = new SimpleColor(color, transparent);
        ColorChanged(_colors[0]);
    }
}