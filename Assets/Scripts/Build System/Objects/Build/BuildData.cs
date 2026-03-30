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

        var localBounds = GetBounds(useLocalPositions: true);
        var worldBounds = GetBounds(useLocalPositions: false);
        var localOffset = new Vector3(localBounds.center.x, localBounds.min.y, localBounds.center.z);
        var worldOffset = new Vector3(worldBounds.center.x, worldBounds.min.y, worldBounds.center.z);
        var centeredPieces = _pieces.Select(piece => CreateCenteredPiece(piece, localOffset, worldOffset)).ToArray();

        return new BuildData(centeredPieces);
    }

    public Bounds GetBounds()
    {
        return GetBounds(useLocalPositions: false);
    }

    private Bounds GetBounds(bool useLocalPositions)
    {
        var hasBounds = false;
        var bounds = default(Bounds);

        foreach (var piece in _pieces)
        {
            var pieceBounds = GetPieceBounds(piece, useLocalPositions);
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

    private static PieceData CreateCenteredPiece(PieceData piece, Vector3 localOffset, Vector3 worldOffset)
    {
        var transientData = piece.TransientData;
        var centeredTransientData = new PieceTransientData(
            transientData.Id,
            transientData.LocalPosition - localOffset,
            transientData.Colors,
            transientData.Rotation,
            transientData.CreationTime,
            transientData.WorldPosition - worldOffset);

        return new PieceData(piece.Template, centeredTransientData);
    }

    private static Bounds GetPieceBounds(PieceData piece, bool useLocalPositions)
    {
        var size = GetRotatedSize(piece.Template.GetSize().ToWorld(), piece.TransientData.Rotation);
        var position = useLocalPositions ? piece.TransientData.LocalPosition : piece.TransientData.WorldPosition;
        return new Bounds(position, size);
    }

    private static Vector3 GetRotatedSize(Vector3 size, PieceRotation rotation)
    {
        if (rotation is PieceRotation.East or PieceRotation.West)
            (size.x, size.z) = (size.z, size.x);

        return size;
    }
}
