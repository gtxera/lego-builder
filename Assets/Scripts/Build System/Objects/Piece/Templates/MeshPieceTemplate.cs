using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class MeshPieceTemplate : IPieceTemplate
{
    [SerializeField]
    private MeshPiece _meshPiecePrefab;

    [SerializeField]
    private PieceTag[] _pieceTags;
    
    public void Configure(GameObject pieceObject)
    {
        Object.Instantiate(_meshPiecePrefab, pieceObject.transform);
    }

    public void OnDestroy(GameObject pieceObject) { }

    public PieceVector GetSize() => _meshPiecePrefab.Size;

    public int GetColorCount() => 1;

    public IEnumerable<Vector3> GetSocketPositions()
    {
        var size = GetSize();
        var halfHeight = size.ToWorld().y / 2;
        var offset = new Vector3((size.X - 1) * .4f, 0, (size.Y - 1) * .4f);

        for (var x = 0; x < size.X; x++)
        for (var y = 0; y < size.Y; y++) 
            yield return new PieceVector(x, y, -halfHeight).ToWorld() - offset;
    }

    public IEnumerable<Vector3> GetStudPositions() => Enumerable.Empty<Vector3>();

    public IEnumerable<PieceTag> GetTags() => _pieceTags ?? Enumerable.Empty<PieceTag>();
}
