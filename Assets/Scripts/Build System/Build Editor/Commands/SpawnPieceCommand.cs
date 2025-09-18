using System;
using UnityEngine;

public class SpawnPieceCommand : ICommand
{
    private readonly Build _build;
    private readonly PieceData _pieceData;
    
    public SpawnPieceCommand(Build build, PieceData pieceData)
    {
        _build = build;
        _pieceData = pieceData;
    }

    public void Commit()
    {
        EventBus<PieceCreatedEvent>.Raise(new PieceCreatedEvent(_build.GetPiece(_pieceData.TransientData.Id)));
    }

    public void Redo()
    {
        var piece = _build.Add(_pieceData);
        EventBus<PieceCreatedEvent>.Raise(new PieceCreatedEvent(piece));
    }

    public void Undo()
    {
        var piece = _build.GetPiece(_pieceData.TransientData.Id);
        EventBus<PieceRemovedEvent>.Raise(new PieceRemovedEvent(piece));
        _build.Remove(piece);
    }
}
