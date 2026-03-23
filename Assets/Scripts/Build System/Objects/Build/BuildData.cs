using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BuildData
{
    [SerializeReference]
    private PieceData[] _pieces;

    public BuildData(PieceData[] pieces)
    {
        _pieces = pieces;
    }

    public IEnumerable<PieceData> Pieces => _pieces;

    public BuildData GetCentered()
    {
        if (_pieces == null || _pieces.Length == 0)
            return new BuildData(Array.Empty<PieceData>());

        var bounds = GetBounds();
        var offset = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        var centeredPieces = _pieces.Select(piece => CreateCenteredPiece(piece, offset)).ToArray();

        return new BuildData(centeredPieces);
    }

    private Bounds GetBounds()
    {
        var hasBounds = false;
        var bounds = default(Bounds);

        foreach (var piece in _pieces)
        {
            var pieceBounds = GetPieceBounds(piece);
            if (!hasBounds)
            {
                bounds = pieceBounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(pieceBounds);
        }

        return bounds;
    }

    private static PieceData CreateCenteredPiece(PieceData piece, Vector3 offset)
    {
        var transientData = piece.TransientData;
        var centeredTransientData = new PieceTransientData(
            transientData.Id,
            transientData.LocalPosition - offset,
            transientData.Colors,
            transientData.Rotation,
            transientData.CreationTime,
            transientData.WorldPosition - offset);

        return new PieceData(piece.Template, centeredTransientData);
    }

    private static Bounds GetPieceBounds(PieceData piece)
    {
        var size = GetRotatedSize(piece.Template.GetSize().ToWorld(), piece.TransientData.Rotation);
        return new Bounds(piece.TransientData.WorldPosition, size);
    }

    private static Vector3 GetRotatedSize(Vector3 size, PieceRotation rotation)
    {
        if (rotation is PieceRotation.East or PieceRotation.West)
            (size.x, size.z) = (size.z, size.x);

        return size;
    }
}
