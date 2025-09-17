using UnityEditor;
using UnityEngine;

public class MeshGenerator
{
    public Mesh GetPieceMesh(PieceVector size, PieceMeshType meshType)
    {
        var worldSize = size.ToWorld();
        var halfWorldSize = worldSize / 2;
        halfWorldSize.x -= 0.001f;
        halfWorldSize.z -= 0.001f;

        var leftFaceCenter = new Vector3(-halfWorldSize.x, 0, 0);
        var rightFaceCenter = new Vector3(halfWorldSize.x, 0, 0);
        var backFaceCenter = new Vector3(0, 0, -halfWorldSize.z);
        var frontFaceCenter = new Vector3(0, 0, halfWorldSize.z);
        var upFaceCenter = new Vector3(0, halfWorldSize.y, 0);
        
        var shape = new Face(
            leftFaceCenter,
            Vector3.left,
            new Vector2(halfWorldSize.z, halfWorldSize.y))
            .Combine(new Face(
                rightFaceCenter,
                Vector3.right, 
                new Vector2(halfWorldSize.z, halfWorldSize.y)))
            .Combine(new Face(
                backFaceCenter,
                Vector3.back,
                new Vector2(halfWorldSize.x, halfWorldSize.y)))
            .Combine(new Face(
                frontFaceCenter,
                Vector3.forward, 
                new Vector2(halfWorldSize.x, halfWorldSize.y)))
            .Combine(new Face(
                upFaceCenter,
                Vector3.up, 
                new Vector2(halfWorldSize.x, halfWorldSize.z)));

        if (meshType == PieceMeshType.WithStuds)
        {
            var offset = new Vector3((size.X - 1) * .4f, 0, (size.Y - 1) * .4f);

            for (var x = 0; x < size.X; x++)
            for (var y = 0; y < size.Y; y++)
            {
                shape = shape.Combine(new Cylinder(
                    new PieceVector(x, y, halfWorldSize.y).ToWorld() - offset,
                    .24f,
                    0.18f,
                    20));
            }
        }
        
        /*var internalLeftFaceCenter = leftFaceCenter + Vector3.right * 0.12f;
        var internalRightFaceCenter = rightFaceCenter + Vector3.left * 0.12f;
        var internalBackFaceCenter = backFaceCenter + Vector3.forward * 0.12f;
        var internalFrontFaceCenter = frontFaceCenter + Vector3.back * 0.12f;
        var internalUpFaceCenter = upFaceCenter + Vector3.down * 0.12f;

        var internalSize = halfWorldSize - Vector3.one * 0.12f;
        internalSize.y = halfWorldSize.y;
        
        shape = shape
            .Combine(new Face(
                internalLeftFaceCenter,
                Vector3.right, 
                new Vector2(internalSize.z, internalSize.y)))
            .Combine(new Face(
                internalRightFaceCenter,
                Vector3.left, 
                new Vector2(internalSize.z, internalSize.y)))
            .Combine(new Face(
                internalFrontFaceCenter,
                Vector3.back,
                new Vector2(internalSize.x, internalSize.y)))
            .Combine(new Face(
                internalBackFaceCenter,
                Vector3.forward, 
                new Vector2(internalSize.x, internalSize.y)))
            .Combine(new Face(
                internalUpFaceCenter,
                Vector3.down, 
                new Vector2(internalSize.x, internalSize.z)));*/
        
        
        var mesh = shape.ToMesh();
        
        return mesh;
    }
}

public enum PieceMeshType
{
    WithStuds,
    WithoutStuds
}