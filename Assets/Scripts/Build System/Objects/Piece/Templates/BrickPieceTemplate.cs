using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class BrickPieceTemplate : IPieceTemplate
{
    [SerializeField]
    private int _width = 2;
    [SerializeField]
    private int _length = 2;
    
    private const float Height = .5f;

    public BrickPieceTemplate(int width, int length)
    {
        _width = width;
        _length = length;
    }
    
    public void Configure(GameObject pieceObject)
    {
        var size = GetSize();
        var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brick.transform.localScale = size.ToWorld() - new Vector3(0.02f, 0.02f, 0.02f);
        brick.transform.SetParent(pieceObject.transform, false);
        Object.Destroy(brick.GetComponent<BoxCollider>());
        brick.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/Piece/DefaultPieceMaterial");
        var collider = pieceObject.AddComponent<BoxCollider>();
        collider.size = size.ToWorld();
    }

    public PieceVector GetSize() => new(_width, _length, Height);

    public int GetColorCount() => 1;
    
    public IEnumerable<Vector3> GetSocketPositions()
    {
        var halfHeight = GetSize().ToWorld().y / 2;
        var offset = new Vector3((_width - 1) * .4f, 0, (_length - 1) * .4f);

        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _length; y++) 
                yield return new PieceVector(x, y, -halfHeight).ToWorld() - offset;
    }

    public IEnumerable<Vector3> GetStudPositions()
    {
        var halfHeight = GetSize().ToWorld().y / 2;
        var offset = new Vector3((_width - 1) * .4f, 0, (_length - 1) * .4f);

        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _length; y++) 
                yield return new PieceVector(x, y, halfHeight).ToWorld() - offset;
    }
}
