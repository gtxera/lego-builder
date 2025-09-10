using System;
using UnityEngine;

[Serializable]
public class MinimumSizeRequirement : IBuildRequirement
{
    [SerializeField]
    private PieceVector _size;
    
    public bool IsSatisfied(Build build)
    {
        var bounds = new Bounds(Vector3.zero, _size.ToWorld());
        var buildBounds = build.GetBounds();

        return buildBounds.Contains(bounds.max) && buildBounds.Contains(bounds.min);
    }

    public string GetText() => string.Empty;
}
