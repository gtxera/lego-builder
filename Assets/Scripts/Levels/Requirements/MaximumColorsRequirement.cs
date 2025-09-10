using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class MaximumColorsRequirement : IBuildRequirement
{
    [SerializeField]
    private int _count;
    
    public bool IsSatisfied(Build build)
    {
        return build.Pieces.Select(piece => piece.Colors[0].NamedColor).ToHashSet().Count < _count;
    }

    public string GetText() => $"Conter menos que {_count} cores";
}
