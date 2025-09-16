using System;
using System.Collections.Generic;
using UnityEngine;

public class Circle : Shape
{
    private readonly List<int> _triangles;

    public Circle(Vector3 center, float radius, int resolution)
    {
        var step = Mathf.PI * 2 * (1f / resolution);

        var verticesArray = new Vector3[resolution + 1];
        verticesArray[0] = center;
        
        for (var i = 1; i < verticesArray.Length; i++)
        {
            var angle = step * i;
            verticesArray[i] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius) + center;
        }

        Vertices = verticesArray;

        _triangles = new List<int>();
        var triangleCount = resolution - 1;

        for (var i = 0; i < triangleCount; i++)
        {
            _triangles.Add(0);
            _triangles.Add(i + 2);
            _triangles.Add(i + 1);
        }
        
        _triangles.Add(0);
        _triangles.Add(1);
        _triangles.Add(resolution );

        Triangles = _triangles;

        var normalsArray = new Vector3[resolution + 1];
        Array.Fill(normalsArray, Vector3.up);

        Normals = normalsArray;
    }

    public override IEnumerable<Vector3> Vertices { get; }
    public override IEnumerable<int> Triangles { get; }
    public override IEnumerable<Vector3> Normals { get; }
}
