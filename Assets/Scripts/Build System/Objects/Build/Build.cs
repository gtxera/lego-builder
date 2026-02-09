using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class Build : MonoBehaviour
{
    private readonly HashSet<Piece> _pieces = new();

    [Inject]
    private BuildEditor _buildEditor;

    public IReadOnlyCollection<Piece> Pieces => _pieces;

    public Piece Add(PieceData pieceData)
    {
        var pieceGameObject = new GameObject("Piece");
        var piece = pieceGameObject.AddComponent<Piece>();
        pieceGameObject.transform.SetParent(transform);
        piece.Initialize(pieceData);
        _pieces.Add(piece);

        return piece;
    }
    
    public Piece Add(IPieceTemplate template)
    {
        var pieceGameObject = new GameObject("Piece");
        var piece = pieceGameObject.AddComponent<Piece>();
        pieceGameObject.transform.SetParent(transform);
        piece.Initialize(template);
        _pieces.Add(piece);

        return piece;
    }

    public void Remove(Piece piece)
    {
        _pieces.Remove(piece);
        Destroy(piece.gameObject);
    }

    public Piece GetPiece(Guid id) => _pieces.SingleOrDefault(piece => piece.Id == id);

    public IEnumerable<Piece> GetPieces(IEnumerable<Guid> ids) => _pieces.Where(piece => ids.Contains(piece.Id));

    public Bounds GetBounds()
    {
        var bounds = new Bounds();

        foreach (var piece in _pieces)
        {
            var pieceBounds = piece.GetBounds();
            pieceBounds.center = piece.transform.position - transform.position;
            bounds.Encapsulate(pieceBounds);
        }

        var angle = transform.eulerAngles.y;
        if (Mathf.Approximately(angle, 90) || Mathf.Approximately(angle, 270))
        {
            var size = bounds.size;
            (size.x, size.z) = (size.z, size.x);
            bounds.size = size;
        }

        return bounds;
    }

    public void Create(BuildData buildData)
    {
        foreach (var piece in buildData.Pieces)
            Add(piece);
    }

    public BuildData GetBuildData()
    {
        var pieces = _pieces.Select(piece => piece.GetData()).OrderBy(data => data.TransientData.CreationTime).ToArray();
        return new BuildData(pieces);
    }

    public bool IsPartOfBuild(Piece piece) => _pieces.Contains(piece);

    private void OnDrawGizmos()
    {
        var bounds = GetBounds();
        var color = Gizmos.color;

        var angle = transform.eulerAngles.y;
        if (Mathf.Approximately(angle, 90) || Mathf.Approximately(angle, 270))
            Gizmos.color = Color.red;

        Gizmos.DrawCube(transform.TransformPoint(bounds.center), bounds.size);

        Gizmos.color = color;
    }
}
