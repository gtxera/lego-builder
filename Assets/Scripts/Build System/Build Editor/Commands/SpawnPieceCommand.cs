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
    }

    public void Redo()
    {
        _build.Add(_pieceData);
    }

    public void Undo()
    {
        var piece = _build.GetPiece(_pieceData.TransientData.Id);
        
        _build.Remove(piece);
    }
}
