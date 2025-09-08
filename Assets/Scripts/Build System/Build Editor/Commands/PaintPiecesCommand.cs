using System;
using System.Collections.Generic;
using UnityEngine;

public class PaintPiecesCommand : ICommand
{
    private readonly Build _build;
    private readonly IReadOnlyDictionary<Guid, PieceColor> _piecesAndOldColors;
    private readonly PieceColor _pieceColor;

    public PaintPiecesCommand(Build build, IReadOnlyDictionary<Guid, PieceColor> piecesAndOldColors, PieceColor pieceColor)
    {
        _build = build;
        _piecesAndOldColors = piecesAndOldColors;
        _pieceColor = pieceColor;
    }

    public void Commit()
    {
        
    }

    public void Redo()
    {
        var pieces = _build.GetPieces(_piecesAndOldColors.Keys);

        foreach (var piece in pieces)
            piece.TrySetColor(_pieceColor, 0);
    }

    public void Undo()
    {
        foreach (var (piece, color) in _piecesAndOldColors)
            _build.GetPiece(piece).TrySetColor(color, 0);
    }
}
