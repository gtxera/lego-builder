using UnityEngine;

public class AnchorPoint : PieceConnector<AnchorPoint, AnchorPoint>
{
    protected override void Awake()
    {
        var collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new PieceVector(1, 1, 0.1f).ToWorld();
        gameObject.layer = LayerMask.NameToLayer("Anchors");
    }

    public bool IsCompatible(AnchorPoint anchor) => anchor.transform.forward == -transform.forward;

    public bool PointsTo(Vector3 normal) => transform.forward == normal;
    public Vector3 GetDistanceToCenter() => transform.localPosition;
    
    protected override bool CanConnect(AnchorPoint anchor) => IsCompatible(anchor);
}
