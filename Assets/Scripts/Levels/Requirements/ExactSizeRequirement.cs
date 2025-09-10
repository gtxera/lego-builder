using System;
using UnityEngine;

[Serializable]
public class ExactSizeRequirement : SizeRequirement
{
    protected override bool SizeCondition(Bounds sizeBounds, Bounds buildBounds) => sizeBounds == buildBounds;
}
