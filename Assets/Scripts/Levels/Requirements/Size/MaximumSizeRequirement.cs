using System;
using UnityEngine;

[Serializable]
public class MaximumSizeRequirement : SizeRequirement
{
    protected override string MaterialResourcePath => "Materials/Size Requirements/Maximum Size";

    public override string GetText() => "Ter um tamanho menor que o indicado";

    protected override bool SizeCondition(Bounds sizeBounds, Bounds buildBounds)
    {
        return sizeBounds.Contains(buildBounds.max) && sizeBounds.Contains(buildBounds.min);
    }
}
