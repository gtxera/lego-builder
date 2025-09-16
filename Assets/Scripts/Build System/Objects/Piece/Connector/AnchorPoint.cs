using UnityEngine;

public class AnchorPoint : PieceConnector<AnchorPoint, AnchorPoint>
{
    protected override string Layer => "Anchors";

    public bool IsCompatible(AnchorPoint anchor) => anchor.GetDirection() == -GetDirection();

    public Vector3 GetDirection() => transform.forward;
    public Vector3 GetDistanceToCenter() => transform.localPosition;
    
    protected override bool CanConnect(AnchorPoint anchor) => IsCompatible(anchor);
}
