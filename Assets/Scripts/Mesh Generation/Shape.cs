using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Shape
{
    protected abstract IEnumerable<Vector3> Vertices { get; }
    protected abstract IEnumerable<int> Triangles { get; }
    protected abstract IEnumerable<Vector3> Normals { get; }
    
    public Mesh ToMesh()
    {
        return new Mesh
        {
            vertices = Vertices.ToArray(),
            triangles = Triangles.ToArray(),
            normals = Normals.ToArray()
        };
    }

    public Shape Combine(Shape shape)
    {
        return new CombinedShape(this, shape);
    }
    
    private class CombinedShape : Shape
    {
        public CombinedShape(Shape shape, Shape other)
        {
            Vertices = shape.Vertices.Concat(other.Vertices);
            Triangles = shape.Triangles.Concat(other.Triangles.Select(index => index + shape.Vertices.Count()));
            Normals = shape.Normals.Concat(other.Normals);
        }
        
        protected override IEnumerable<Vector3> Vertices { get; }
        protected override IEnumerable<int> Triangles { get; }
        protected override IEnumerable<Vector3> Normals { get; }
    }
}
