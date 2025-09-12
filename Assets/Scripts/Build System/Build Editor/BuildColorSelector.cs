using System.Collections.Generic;
using UnityEngine;

public class BuildColorSelector
{
    private readonly List<PieceColor> _colors = new();

    public BuildColorSelector()
    {
        _colors = new();
        _colors.Add(new SimpleColor(Color.white));
    }

    public PieceColor GetSelectedColorFor(int index) => _colors[index];

    public void SetColor(Color color) => _colors[0] = new SimpleColor(color);
}