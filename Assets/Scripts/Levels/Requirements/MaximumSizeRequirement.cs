using System;
using UnityEngine;

[Serializable]
public class MaximumSizeRequirement : SizeRequirement
{
    protected override bool SizeCondition(Bounds sizeBounds, Bounds buildBounds)
    {
        return sizeBounds.Contains(buildBounds.max) && sizeBounds.Contains(buildBounds.min);
    }
}
