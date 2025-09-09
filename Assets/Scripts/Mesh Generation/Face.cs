using System.Collections.Generic;
using UnityEngine;

public class Face : Shape
{
    
    public Face(Vector3 center, Vector3 normal, Vector2 extents)
    {
        var right = Vector3.Cross(normal, Vector3.up);
        var up = Vector3.Cross(right, normal);
        
        Vertices = new Vector3[]
        {
            
        };
    }

    protected override IEnumerable<Vector3> Vertices { get; }
    protected override IEnumerable<int> Triangles { get; }
    protected override IEnumerable<Vector3> Normals { get; }
}
