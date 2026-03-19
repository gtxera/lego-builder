using System;
using System.Collections.Generic;
using UnityEngine;

public class MovePiecesCommand : ICommand
{
    private readonly Build _build;
    private readonly IReadOnlyDictionary<Guid, (Vector3 StartPosition, Vector3 FinalPosition)> _piecePositions;

    public MovePiecesCommand(Build build, IReadOnlyDictionary<Guid, (Vector3 StartPosition, Vector3 FinalPosition)> piecePositions)
    {
        _build = build;
        _piecePositions = piecePositions;
    }

    public void Commit()
    {
        foreach (var (pieceId, positions) in _piecePositions)
        {
            var piece = _build.GetPiece(pieceId);
            EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, positions.StartPosition, positions.FinalPosition));
        }
    }

    public void Redo()
    {
        foreach (var (pieceId, positions) in _piecePositions)
        {
            var piece = _build.GetPiece(pieceId);
            piece.MoveTo(positions.FinalPosition);
            EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, positions.StartPosition, positions.FinalPosition));
        }
    }

    public void Undo()
    {
        foreach (var (pieceId, positions) in _piecePositions)
        {
            var piece = _build.GetPiece(pieceId);
            piece.MoveTo(positions.StartPosition);
            EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, positions.FinalPosition, positions.StartPosition));
        }
    }
}
