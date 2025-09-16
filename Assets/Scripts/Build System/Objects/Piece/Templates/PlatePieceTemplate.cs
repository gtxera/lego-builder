using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class PlatePieceTemplate : IPieceTemplate
{
    [SerializeField]
    private int _width = 2;
    [SerializeField]
    private int _length = 2;
    
    private const float Height = .32f;

    public PlatePieceTemplate(int width, int length)
    {
        _width = width;
        _length = length;
    }
    
    public void Configure(GameObject pieceObject)
    {
        var size = GetSize();

        var body = PieceCreationHelper.MakeBody(size);
        body.transform.SetParent(pieceObject.transform, false);
        
        var collider = pieceObject.AddComponent<BoxCollider>();
        collider.size = size.ToWorld();

        foreach (var studPosition in GetStudPositions())
        {
            var stud = PieceCreationHelper.MakeStud(studPosition);
            stud.transform.SetParent(pieceObject.transform, false);
            stud.transform.localPosition = studPosition;
        }
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
