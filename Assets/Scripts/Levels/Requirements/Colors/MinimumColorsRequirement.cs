using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class MinimumColorsRequirement : IBuildRequirement
{
    [SerializeField]
    private int _count;
    
    public bool IsSatisfied(Build build)
    {
        return build.Pieces.Select(piece => piece.Colors[0].NamedColor).ToHashSet().Count >= _count;
    }

    public string GetText() => $"Conter no mínimo {_count} cores";
}
