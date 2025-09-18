using System;
using UnityEngine;

public class MovePieceCommand : ICommand
{
    private readonly Build _build;
    private readonly Guid _pieceId;
    private readonly Vector3 _startPosition;
    private readonly Vector3 _finalPosition;

    public MovePieceCommand(Build build, Guid pieceId, Vector3 startPosition, Vector3 finalPosition)
    {
        _pieceId = pieceId;
        _startPosition = startPosition;
        _finalPosition = finalPosition;
        _build = build;
    }

    public void Commit()
    {
        var piece = _build.GetPiece(_pieceId);
        
        EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, _startPosition, _finalPosition));
    }

    public void Redo()
    {
        var piece = _build.GetPiece(_pieceId);
        piece.MoveTo(_finalPosition);
        
        EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, _startPosition, _finalPosition));
    }

    public void Undo()
    {
        var piece = _build.GetPiece(_pieceId);
        piece.MoveTo(_startPosition);
        
        EventBus<PieceMovedEvent>.Raise(new PieceMovedEvent(piece, _finalPosition, _startPosition));
    }
}
