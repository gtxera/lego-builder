using KBCore.Refs;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MeshPiece : MonoBehaviour
{
    [field: SerializeField]
    public PieceVector Size { get; private set; }

    [SerializeField]
    private Transform _meshesPositionRoot;

    [SerializeField]
    private Transform _meshesScaleRoot;

    [SerializeField, Self]
    private BoxCollider _boxCollider;

    private void OnValidate()
    {
        #if UNITY_EDITOR
        this.ValidateRefs();
        var meshFilters = GetComponentsInChildren<MeshFilter>();
        
        var meshesBounds = new Bounds();
        foreach (var meshFilter in meshFilters)
            meshesBounds.Encapsulate(meshFilter.sharedMesh.bounds);
        
        var desiredSize = Size.ToWorld();
        var renderersSize = meshesBounds.size;
        
        var scale = new Vector3(desiredSize.x / renderersSize.x, desiredSize.y / renderersSize.y, desiredSize.z / renderersSize.z);
        _meshesScaleRoot.localScale = scale;
        
        _meshesPositionRoot.localPosition = -meshesBounds.center;
        
        _boxCollider.size = desiredSize;
        #endif
    }
}
