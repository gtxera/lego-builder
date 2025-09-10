using System;
using UnityEngine;

[Serializable]
public class MaximumSizeRequirement : IBuildRequirement
{
    [SerializeField]
    private PieceVector _size;
    
    public bool IsSatisfied(Build build)
    {
        var bounds = new Bounds(Vector3.zero, _size.ToWorld());
        var buildBounds = build.GetBounds();

        return bounds.Contains(buildBounds.max) && bounds.Contains(buildBounds.min);
    }

    public string GetText() => string.Empty;
}
