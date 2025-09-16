using System;
using UnityEngine;

[Serializable]
public class MinimumPieceCountRequirement : IBuildRequirement
{
    [SerializeField]
    private int _count;
    
    public bool IsSatisfied(Build build)
    {
        return build.Pieces.Count >= _count;
    }

    public string GetText() => $"Conter no mínimo {_count} peças";
}
