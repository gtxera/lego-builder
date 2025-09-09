using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class BrickPieceTemplate : IPieceTemplate
{
    [SerializeField]
    private int _width = 3;
    [SerializeField]
    private int _length = 3;
    [SerializeField]
    private float _height = .5f;
    
    public void Configure(GameObject pieceObject)
    {
        var size = GetSize();
        var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brick.transform.localScale = size.ToWorld() - new Vector3(0.02f, 0.02f, 0.02f);
        brick.transform.SetParent(pieceObject.transform, false);
        Object.Destroy(brick.GetComponent<BoxCollider>());
        var collider = pieceObject.AddComponent<BoxCollider>();
        collider.size = size.ToWorld();
    }

    public PieceVector GetSize() => new(_width, _length, _height);
    
    public int GetColorCount() => 1;
    
    public IEnumerable<Vector3> GetSocketPositions()
    {
        var halfSize = GetSize().ToWorld() / 4;
        var centerOffset = new Vector3(1, 0, 1) * .2f;
        
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _length; y++) 
                yield return halfSize - new PieceVector(x, y, 3 * halfSize.y).ToWorld() + centerOffset;
    }

    public IEnumerable<Vector3> GetStudPositions()
    {
        var halfSize = GetSize().ToWorld() / 4;
        var centerOffset = new Vector3(1, 0, 1) * .2f;
        
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _length; y++) 
                yield return halfSize - new PieceVector(x, y, -halfSize.y).ToWorld() + centerOffset;
    }
}
