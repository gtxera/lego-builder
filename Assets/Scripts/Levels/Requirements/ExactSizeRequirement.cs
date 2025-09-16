using System;
using UnityEngine;

[Serializable]
public class ExactSizeRequirement : SizeRequirement
{
    protected override string MaterialResourcePath => "Materials/Size Requirements/Exact Size";

    public override string GetText() => "Ter exatamente o tamanho indicado";

    protected override bool SizeCondition(Bounds sizeBounds, Bounds buildBounds)
    {
        var centerDifference = buildBounds.center - sizeBounds.center;
        var extentsDifference = buildBounds.extents - sizeBounds.extents;

        var maximumMagnitudeDifference = new Vector3(0.05f, 0.05f, 0.05f).sqrMagnitude;

        return centerDifference.sqrMagnitude < maximumMagnitudeDifference &&
               extentsDifference.sqrMagnitude < maximumMagnitudeDifference;
    }
}
