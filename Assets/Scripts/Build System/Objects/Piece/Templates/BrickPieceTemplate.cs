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
    [SerializeField]
    private float _height = .5f;
    
    public void Configure(GameObject pieceObject)
    {
        var size = GetSize();
        var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brick.transform.localScale = size;
        brick.transform.SetParent(pieceObject.transform, false);
        Object.Destroy(brick.GetComponent<BoxCollider>());
        var collider = pieceObject.AddComponent<BoxCollider>();
        collider.size = size;
    }

    public Vector3 GetSize() => new(_width, _height, _length);
    
    public int GetColorCount() => 1;
    
    public IEnumerable<Vector3> GetSocketPositions()
    {
        var halfSize = GetSize() / 4;
        
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _length; y++) 
                yield return halfSize - new Vector3(x, 3 * halfSize.y, y);
    }

    public IEnumerable<Vector3> GetStudPositions()
    {
        var halfSize = GetSize() / 4;
        
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _length; y++) 
                yield return halfSize - new Vector3(x, -halfSize.y, y);
    }
}
