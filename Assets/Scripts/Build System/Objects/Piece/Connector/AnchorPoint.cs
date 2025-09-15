using UnityEngine;

public class AnchorPoint : PieceConnector<AnchorPoint, AnchorPoint>
{
    protected override string Layer => "Anchors";

    public bool IsCompatible(AnchorPoint anchor) => anchor.transform.forward == -transform.forward;

    public bool PointsTo(Vector3 normal) => transform.forward == normal;
    public Vector3 GetDistanceToCenter() => transform.localPosition;
    
    protected override bool CanConnect(AnchorPoint anchor) => IsCompatible(anchor);
}
