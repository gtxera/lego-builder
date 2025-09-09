using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;

    private void Awake()
    {
        var face = new Cylinder(Vector3.zero, 1, 1, 5);
        _meshFilter.mesh = face.ToMesh();
    }
}
