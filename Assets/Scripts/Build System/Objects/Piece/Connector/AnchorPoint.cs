using System;
using UnityEngine;
using UnityEngine.Scripting;

public class AnchorPoint : PieceConnector<AnchorPoint, AnchorPoint>
{
    protected override string Layer => "Anchors";

    public bool IsCompatible(AnchorPoint anchor) => anchor.GetDirection() == -GetDirection();

    public Vector3 GetDirection() => transform.forward;
    public Vector3 GetDistanceToCenter() => transform.localPosition;
    
    protected override bool CanConnect(AnchorPoint anchor) => IsCompatible(anchor);
    
    [Preserve] private void UsedOnlyForAOTCodeGeneration()
    {
        var a = TryGetComponent<AnchorPoint>(out var b); 
        throw new Exception("This method is used for AOT code generation only. Do not call it at runtime.");
    }
}
