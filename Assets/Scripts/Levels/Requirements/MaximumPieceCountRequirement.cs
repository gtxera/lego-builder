using System;
using UnityEngine;

[Serializable]
public class MaximumPieceCountRequirement : IBuildRequirement
{
    [SerializeField]
    private int _count;
    
    public bool IsSatisfied(Build build)
    {
        return build.Pieces.Count < _count;
    }

    public string GetText() => $"Conter menos que {_count} peças";
}
