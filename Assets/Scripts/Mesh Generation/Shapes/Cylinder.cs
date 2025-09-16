using System.Collections.Generic;
using UnityEngine;

public class Cylinder : Shape
{
    private readonly Shape _finalShape;

    public Cylinder(Vector3 bottomOrigin, float radius, float height, int resolution)
    {
        var circle = new Circle(bottomOrigin + Vector3.up * height, radius, resolution);
        _finalShape = circle;

        var step = Mathf.PI * 2 * (1f / resolution);
        var halfStep = step / 2f;
        var halfHeight = height / 2f;

        foreach (var vertex in circle.Vertices)
        {
            var position = new Vector3(vertex.x, 0, vertex.z);
            var normal = position.normalized;
            var size = new Vector2(halfStep, halfHeight);
            position.y = halfHeight;
            var right = Vector3.Cross(Vector3.up, normal);
            var up = Vector3.Cross(normal, right);
            var rotation = Quaternion.AngleAxis(step * Mathf.Rad2Deg, up);
            var offset = rotation * size;
            position += offset;
            _finalShape = _finalShape.Combine(new Face(position, normal, size));
        }
    }

    public override IEnumerable<Vector3> Vertices => _finalShape.Vertices;
    public override IEnumerable<int> Triangles => _finalShape.Triangles;
    public override IEnumerable<Vector3> Normals => _finalShape.Normals;
}
