using System;
using UnityEngine;

[Serializable]
public class MinimumSizeRequirement : SizeRequirement
{
    protected override string MaterialResourcePath => "Materials/Size Requirements/Minimum Size";

    public override string GetText() => "Ter um tamanho maior que o indicado";

    private readonly Vector3 _sizeLeeway = new Vector3(0.005f, 0.005f, 0.005f);
    protected override bool SizeCondition(Bounds sizeBounds, Bounds buildBounds)
    {
        return buildBounds.Contains(sizeBounds.max - _sizeLeeway) && buildBounds.Contains(sizeBounds.min + _sizeLeeway);
    }
}
