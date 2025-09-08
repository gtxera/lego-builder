using System.Collections.Generic;
using UnityEngine;

public class RemoverTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly ScreenRaycaster _screenRaycaster;

    private readonly List<PieceData> _removedPieces = new();

    public RemoverTool(BuildEditor buildEditor, ScreenRaycaster screenRaycaster)
    {
        _buildEditor = buildEditor;
        _screenRaycaster = screenRaycaster;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        _removedPieces.Clear();
        
        var ray = _screenRaycaster.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit, 10f))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();
        
        if (piece == null)
            return;
        
        RemovePiece(piece);
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        if (_removedPieces.Count == 0)
            return;
        
        var command = new RemovePiecesCommand(_buildEditor.Build, _removedPieces.ToArray());
        
        _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        var ray = _screenRaycaster.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit, 10f))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();
        
        if (piece == null)
            return;
        
        RemovePiece(piece);
    }

    public void Tap() { }

    private void RemovePiece(Piece piece)
    {
        _removedPieces.Add(piece.GetData());
        _buildEditor.Build.Remove(piece);
    }
}
