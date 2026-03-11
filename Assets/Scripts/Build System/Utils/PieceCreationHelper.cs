using System.Collections.Generic;
using UnityEngine;

public static class PieceCreationHelper
{
    private static Material _defaultPieceMaterial;
    private static Mesh _cubeMesh;
    private static Mesh _cylinderMesh;
    private static Mesh _rampMesh;
    private static List<Vector3> _vertices = new();
    
    public static Mesh GetBody(PieceVector size, Vector3 position = new())
    {
        if (_cubeMesh == null)
            _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        var body = Object.Instantiate(_cubeMesh);
        ScaleMesh(body, size.ToWorld() - new Vector3(0.02f, 0, 0.02f));
        if (position != Vector3.zero)
            MoveMesh(body, position);

        return body;
    }

    public static Mesh GetStud(Vector3 position)
    {
        if (_cylinderMesh == null)
            _cylinderMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");

        var stud = Object.Instantiate(_cylinderMesh);
        ScaleMesh(stud, new Vector3(0.48f, 0.18f, 0.48f));
        MoveMesh(stud, position);
            
        return stud;
    }

    public static Mesh GetRamp(PieceVector size, Vector3 position = new())
    {
        if (_rampMesh == null)
            _rampMesh = Resources.Load<Mesh>("Pieces/Primitives/ramp");
        
        var ramp = Object.Instantiate(_rampMesh);
        ScaleMesh(ramp, size.ToWorld() - new Vector3(0.02f, 0, 0.02f));
        if (position != Vector3.zero)
            MoveMesh(ramp, position);
        
        return ramp;
    }

    public static Material GetPieceMaterial()
    {
        if (_defaultPieceMaterial == null)
            _defaultPieceMaterial = Resources.Load<Material>("Materials/Piece/DefaultPieceMaterial");

        return _defaultPieceMaterial;
    }

    private static void ScaleMesh(Mesh mesh, Vector3 scale)
    {
        mesh.GetVertices(_vertices);

        for (var i = 0; i < _vertices.Count; i++)
        {
            var vertex = _vertices[i];
            vertex.x *= scale.x;
            vertex.y *= scale.y;
            vertex.z *= scale.z;
            _vertices[i] = vertex;
        }
        
        mesh.SetVertices(_vertices);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private static void MoveMesh(Mesh mesh, Vector3 position)
    {
        mesh.GetVertices(_vertices);

        for (var i = 0; i < _vertices.Count; i++)
        {
            var vertex = _vertices[i];
            vertex.x += position.x;
            vertex.y += position.y;
            vertex.z += position.z;
            _vertices[i] = vertex;
        }
        
        mesh.SetVertices(_vertices);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
