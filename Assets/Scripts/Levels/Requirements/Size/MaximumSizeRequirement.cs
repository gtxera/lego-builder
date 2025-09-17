using System;
using UnityEngine;

[Serializable]
public class MaximumSizeRequirement : SizeRequirement
{
    protected override string MaterialResourcePath => "Materials/Size Requirements/Maximum Size";

    public override string GetText() => "Ter um tamanho menor que o indicado";

    private readonly Vector3 _sizeLeeway = new Vector3(0.005f, 0.005f, 0.005f);

    protected override bool SizeCondition(Bounds sizeBounds, Bounds buildBounds)
    {
        return sizeBounds.Contains(buildBounds.max - _sizeLeeway) && sizeBounds.Contains(buildBounds.min + _sizeLeeway);
    }
}
