using System;
using System.Collections.Generic;
using UnityEngine;

public class SinglePieceTarget : IEditablePieceTarget
{
    private readonly Build _build;

    private readonly Piece _piece;
    private Vector3 _initialPosition;
    private PieceRotation _initialRotation;

    public SinglePieceTarget(Build build, Piece piece)
    {
        _build = build;
        _piece = piece;
    }

    public bool CanRotate => true;
    public Piece ReferencePiece => _piece;

    public void BeginMove(Piece referencePiece)
    {
        _initialPosition = _piece.transform.position;
        _initialRotation = _piece.Rotation;
        _piece.BeginDragging();
    }

    public void UpdateMove(Vector3 targetPosition)
    {
        _piece.MoveTo(targetPosition);
    }

    public ICommand EndMove()
    {
        _piece.EndDragging();

        if (_piece.transform.position == _initialPosition && _piece.Rotation == _initialRotation)
            return null;

        return new TransformPiecesCommand(_build, new Dictionary<Guid, (Vector3, Vector3, PieceRotation, PieceRotation)>
        {
            { _piece.Id, (_initialPosition, _piece.transform.position, _initialRotation, _piece.Rotation) }
        });
    }

    public void RotateClockwise()
    {
        _piece.RotateClockwise();
    }

    public ICommand Paint(PieceColor color)
    {
        if (_piece.Colors[0].IsEqual(color))
            return null;

        var oldColors = new Dictionary<Guid, PieceColor> { { _piece.Id, _piece.Colors[0] } };
        _piece.TrySetColor(color, 0);
        return new PaintPiecesCommand(_build, oldColors, color);
    }

    public ICommand Remove()
    {
        var pieceData = _piece.GetData();
        EventBus<PieceRemovedEvent>.Raise(new PieceRemovedEvent(_piece));
        _build.Remove(_piece);
        return new RemovePiecesCommand(_build, new[] { pieceData });
    }
}
