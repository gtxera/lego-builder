using System;
using System.Collections.Generic;
using UnityEngine;

public class PainterTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly BuildColorSelector _buildColorSelector;
    private readonly CameraServices _cameraServices;

    private readonly Dictionary<Guid, PieceColor> _coloredPiecesIds = new();

    public PainterTool(BuildColorSelector buildColorSelector, CameraServices cameraServices, BuildEditor buildEditor)
    {
        _buildColorSelector = buildColorSelector;
        _cameraServices = cameraServices;
        _buildEditor = buildEditor;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        _coloredPiecesIds.Clear();

        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();
        
        if (piece == null || !_buildEditor.Build.IsPartOfBuild(piece))
            return;
        
        PaintPiece(piece);
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        if (_coloredPiecesIds.Count == 0)
            return;
        
        var command = new PaintPiecesCommand(
            _buildEditor.Build,
            new Dictionary<Guid, PieceColor>(_coloredPiecesIds),
            _buildColorSelector.GetSelectedColorFor(0));
        
        _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();
        
        if (piece == null || !_buildEditor.Build.IsPartOfBuild(piece))
            return;
        
        PaintPiece(piece);
    }

    private void PaintPiece(Piece piece)
    {
        if (_coloredPiecesIds.ContainsKey(piece.Id))
            return;
        
        var selectedColor = _buildColorSelector.GetSelectedColorFor(0);
        
        if (piece.Colors[0].IsEqual(selectedColor))
            return;
        
        _coloredPiecesIds.Add(piece.Id, piece.Colors[0]);
        piece.TrySetColor(selectedColor, 0);
    }
    
    public void Tap(Vector2 pointerScreenPosition) { }
    public Sprite GetIcon() => Resources.Load<Sprite>("Icons/Brush");
}
