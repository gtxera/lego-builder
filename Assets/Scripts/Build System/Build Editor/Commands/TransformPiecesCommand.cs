using System;
using System.Collections.Generic;
using UnityEngine;

public class TransformPiecesCommand : ICommand
{
    private readonly Build _build;
    private readonly IReadOnlyDictionary<Guid, (Vector3 StartPosition, Vector3 FinalPosition, PieceRotation StartRotation, PieceRotation FinalRotation)> _pieceStates;

    public TransformPiecesCommand(
        Build build,
        IReadOnlyDictionary<Guid, (Vector3 StartPosition, Vector3 FinalPosition, PieceRotation StartRotation, PieceRotation FinalRotation)> pieceStates)
    {
        _build = build;
        _pieceStates = pieceStates;
    }

    public void Commit()
    {
        foreach (var (pieceId, state) in _pieceStates)
        {
            var piece = _build.GetPiece(pieceId);
            if (piece == null)
                continue;

            EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, state.StartPosition, state.FinalPosition));
        }
    }

    public void Redo()
    {
        foreach (var (pieceId, state) in _pieceStates)
        {
            var piece = _build.GetPiece(pieceId);
            if (piece == null)
                continue;

            piece.SetRotation(state.FinalRotation);
            piece.MoveTo(state.FinalPosition);
            EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, state.StartPosition, state.FinalPosition));
        }
    }

    public void Undo()
    {
        foreach (var (pieceId, state) in _pieceStates)
        {
            var piece = _build.GetPiece(pieceId);
            if (piece == null)
                continue;

            piece.SetRotation(state.StartRotation);
            piece.MoveTo(state.StartPosition);
            EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, state.FinalPosition, state.StartPosition));
        }
    }
}
