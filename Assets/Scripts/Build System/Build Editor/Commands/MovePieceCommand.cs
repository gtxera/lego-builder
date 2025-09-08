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
        
    }

    public void Redo()
    {
        _build.GetPiece(_pieceId).MoveTo(_finalPosition);
    }

    public void Undo()
    {
        _build.GetPiece(_pieceId).MoveTo(_startPosition);
    }
}
