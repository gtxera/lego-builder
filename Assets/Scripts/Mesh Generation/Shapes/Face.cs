using System.Collections.Generic;
using UnityEngine;

public class Face : Shape
{
    public Face(Vector3 center, Vector3 normal, Vector2 extents)
    {
        var reference = Mathf.Abs(normal.y) < .9f ? Vector3.up : Vector3.forward;
        var right = Vector3.Cross(reference, normal);
        var up = Vector3.Cross(normal, right);
        
        Vertices = new Vector3[]
        {
            center - (up * extents.y) - (right * extents.x),
            center + (up * extents.y) - (right * extents.x),
            center + (up * extents.y) + (right * extents.x),
            center - (up * extents.y) + (right * extents.x),
        };

        Triangles = new int[]
        {
            0, 3, 1,
            2, 1, 3
        };

        Normals = new Vector3[]
        {
            normal,
            normal,
            normal,
            normal,
        };
    }

    public override IEnumerable<Vector3> Vertices { get; }
    public override IEnumerable<int> Triangles { get; }
    public override IEnumerable<Vector3> Normals { get; }
}
