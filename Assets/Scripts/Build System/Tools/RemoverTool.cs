using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoverTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly CameraServices _cameraServices;

    private readonly HashSet<PieceData> _removedPieces = new();

    public RemoverTool(BuildEditor buildEditor, CameraServices cameraServices)
    {
        _buildEditor = buildEditor;
        _cameraServices = cameraServices;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        _removedPieces.Clear();
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();
        
        if (piece == null || !_buildEditor.Build.IsPartOfBuild(piece))
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
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();
        
        if (piece == null || !_buildEditor.Build.IsPartOfBuild(piece))
            return;
        
        RemovePiece(piece);
    }

    public void Tap(Vector2 pointerScreenPosition) { }
    public Sprite GetIcon() => Resources.Load<Sprite>("Icons/Remove");

    private void RemovePiece(Piece piece)
    {
        _removedPieces.Add(piece.GetData());
        _buildEditor.Build.Remove(piece);
    }
}
