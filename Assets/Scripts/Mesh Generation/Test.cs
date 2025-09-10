using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;

    [SerializeReference, SubclassInstance]
    private IBuildRequirement[] _requirement;

    private void Awake()
    {
        var face = new Cylinder(Vector3.zero, 1, 1, 5);
        _meshFilter.mesh = face.ToMesh();
    }
}
