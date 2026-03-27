using System;
using System.Collections.Generic;
using UnityEngine;

public class PaintPiecesCommand : ICommand
{
    private readonly Build _build;
    private readonly IReadOnlyDictionary<Guid, PieceColor> _piecesAndOldColors;
    private readonly PieceColor _pieceColor;
    private readonly BuildColorSelector _buildColorSelector;
    private readonly BuildColorController _buildColorController;
    private readonly ColorSelectorState? _oldSelectorState;
    private readonly ColorSelectorState? _newSelectorState;

    public PaintPiecesCommand(Build build, IReadOnlyDictionary<Guid, PieceColor> piecesAndOldColors, PieceColor pieceColor)
    {
        _build = build;
        _piecesAndOldColors = piecesAndOldColors;
        _pieceColor = BuildColorSelector.Clone(pieceColor);
    }

    public PaintPiecesCommand(
        Build build,
        IReadOnlyDictionary<Guid, PieceColor> piecesAndOldColors,
        PieceColor pieceColor,
        BuildColorSelector buildColorSelector,
        BuildColorController buildColorController,
        ColorSelectorState oldSelectorState,
        ColorSelectorState newSelectorState)
    {
        _build = build;
        _piecesAndOldColors = piecesAndOldColors;
        _pieceColor = BuildColorSelector.Clone(pieceColor);
        _buildColorSelector = buildColorSelector;
        _buildColorController = buildColorController;
        _oldSelectorState = oldSelectorState;
        _newSelectorState = newSelectorState;
    }

    public void Commit()
    {
        
    }

    public void Redo()
    {
        var pieces = _build.GetPieces(_piecesAndOldColors.Keys);

        foreach (var piece in pieces)
            piece.TrySetColor(_pieceColor, 0);

        RestoreSelectorState(_newSelectorState);
    }

    public void Undo()
    {
        foreach (var (piece, color) in _piecesAndOldColors)
            _build.GetPiece(piece).TrySetColor(color, 0);

        RestoreSelectorState(_oldSelectorState);
    }

    private void RestoreSelectorState(ColorSelectorState? state)
    {
        if (!state.HasValue)
            return;

        _buildColorSelector?.SetColor(state.Value.Color, state.Value.Transparent);
        _buildColorController?.RestoreSelectorState(state.Value);
    }
}
