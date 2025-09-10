using System;
using UnityEngine;

[Serializable]
public class MinimumSizeRequirement : SizeRequirement
{
    protected override bool SizeCondition(Bounds sizeBounds, Bounds buildBounds)
    {
        return buildBounds.Contains(sizeBounds.max) && buildBounds.Contains(sizeBounds.min);
    }
}
