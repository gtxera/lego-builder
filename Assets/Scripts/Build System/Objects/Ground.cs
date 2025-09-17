using KBCore.Refs;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(BoxCollider))]
public class Ground : MonoBehaviour
{
    [SerializeField]
    private int _width;

    [SerializeField]
    private int _length;

    [SerializeField]
    private Mesh _groundMesh;

    [SerializeField, Self]
    private BoxCollider _collider;
    
    [SerializeField, Self]
    private MeshFilter _meshFilter;
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        this.ValidateRefs();
        
        var offset = new Vector3((_width - 1) * 1.6f, 0, (_length - 1) * 1.6f) + new Vector3(_width % 2 == 0 ? 0 : 1.6f, 0, _length % 2 == 0 ? 0 : 1.6f);

        var meshes = new CombineInstance[_width * _length];
        
        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _length; y++)
        {
            var position = new Vector3(x * 3.2f, -.5f, y * 3.2f) - offset;
            var scale = new Vector3(3.2f, 1, 3.2f);

            var matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);

            meshes[x * _length + y] = new CombineInstance
            {
                mesh = _groundMesh,
                transform = matrix
            };
        }

        EditorApplication.delayCall += () =>
        {
            var mesh = new Mesh();
            mesh.CombineMeshes(meshes);
            _meshFilter.sharedMesh = mesh;
        };

        _collider.size = new PieceVector(_width * 4, _length * 4, 1).ToWorld();
    }
    #endif
}
