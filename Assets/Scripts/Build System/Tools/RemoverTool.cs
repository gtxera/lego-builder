using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemoverTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly CameraServices _cameraServices;
    private readonly EditablePieceTargetResolver _editablePieceTargetResolver;
    private readonly BuildSelection _buildSelection;

    private readonly HashSet<PieceData> _removedPieces = new();
    private ICommand _pendingCommand;
    private bool _selectionInteraction;

    public RemoverTool(
        BuildEditor buildEditor,
        CameraServices cameraServices,
        EditablePieceTargetResolver editablePieceTargetResolver,
        BuildSelection buildSelection)
    {
        _buildEditor = buildEditor;
        _cameraServices = cameraServices;
        _editablePieceTargetResolver = editablePieceTargetResolver;
        _buildSelection = buildSelection;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        _removedPieces.Clear();
        _pendingCommand = null;
        _selectionInteraction = false;
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();
        
        if (piece == null || !_buildEditor.Build.IsPartOfBuild(piece))
            return;

        if (_buildSelection.Contains(piece))
        {
            var target = _editablePieceTargetResolver.Resolve(piece);
            _pendingCommand = target?.Remove();
            _selectionInteraction = _pendingCommand != null;
            return;
        }

        RemovePiece(piece);
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        if (_selectionInteraction)
        {
            if (_pendingCommand != null)
                _buildEditor.Commit(_pendingCommand);

            _pendingCommand = null;
            _selectionInteraction = false;
            return;
        }

        if (_removedPieces.Count == 0)
            return;
        
        var command = new RemovePiecesCommand(_buildEditor.Build, _removedPieces.ToArray());
        
        _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        if (_selectionInteraction)
            return;

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
        EventBus<PieceRemovedEvent>.Raise(new PieceRemovedEvent(piece));
        _buildEditor.Build.Remove(piece);
    }
}
