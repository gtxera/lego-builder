using System;
using UnityEngine;

[Serializable]
public class MinimumSizeRequirement : SizeRequirement
{
    protected override string MaterialResourcePath => "Materials/Size Requirements/Minimum Size";

    public override string GetText() => "Ter um tamanho maior que o indicado";

    protected override bool SizeCondition(Bounds sizeBounds, Bounds buildBounds)
    {
        return buildBounds.Contains(sizeBounds.max) && buildBounds.Contains(sizeBounds.min);
    }
}
