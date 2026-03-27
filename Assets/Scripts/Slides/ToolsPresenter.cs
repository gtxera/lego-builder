using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;

public class ToolsPresenter : MonoBehaviour
{
    [Inject]
    private readonly ToolController _toolController;

    [Inject]
    private readonly IEnumerable<ITool> _tools;

    [Inject]
    private readonly BuildEditor _buildEditor;
    
    [Inject]
    private readonly BuildColorSelector _buildColorSelector;
    
    [Inject]
    private readonly BuildTemplateSelector _buildTemplateSelector;

    [Inject]
    private readonly EditablePieceTargetResolver _editablePieceTargetResolver;

    [SerializeField]
    private Build _build;

    [SerializeField]
    private JsonBuild _jsonBuild;

    private LinkedList<ITool> _toolsList;
    private LinkedListNode<ITool> _currentTool;

    private Guid _pieceId;

    private Tween _currentDelay;
    
    private void Awake()
    {
        _toolsList = new LinkedList<ITool>(_tools.Where(tool => tool.GetType() != typeof(SelectionTool)));
    }

    public void StartBuild()
    {
        _buildEditor.StartEditing(_build);
        _toolController.DeselectTool();
    }

    public void StopBuild()
    {
        _buildEditor.FinishEditing();
    }

    public void ShowTool()
    {
        if (_currentTool == null)
            _currentTool = _toolsList.First;
        else if (_currentTool.Next != null)
            _currentTool = _currentTool.Next;

        var tool = _currentTool!.Value;
        
        _toolController.PickTool(tool);

        switch (tool)
        {
            case MoverTool:
                MovePiece();
                break;
            case PainterTool:
                PaintPiece();
                break;
            case RemoverTool:
                RemovePiece();
                break;
            case SpawnerTool:
                SpawnPiece();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tool));
        }
    }

    public void UndoTool()
    {
        _buildEditor.Undo();
        _buildEditor.Undo();
        _currentTool = _currentTool.Previous?.Previous;
    }

    public void CycleUndoRedo()
    {
        if (_currentDelay.isAlive)
            return;
        
        Undo();
    }

    public void StopUndoRedo()
    {
        _currentDelay.Stop();

        while (_buildEditor.RedoIsAvailable)
            _buildEditor.Redo();
    }

    public void Reenter()
    {
        if (_buildEditor.Build == _build)
            return;
        
        _buildEditor.StartEditing(_build);
        _currentTool = null;
        ShowTool();
        ShowTool();
        ShowTool();
        ShowTool();
        CycleUndoRedo();
    }

    public void ShowFinishedBuild()
    {
        _build.Clear();
        _build.CreateLocal(_jsonBuild.GetBuildData().GetCentered());
    }

    public void ClearFinishedBuild()
    {
        _build.Clear();
    }

    private void Undo()
    {
        _buildEditor.Undo();
        _currentDelay = Tween.Delay(1f, () =>
        {
            if (!_buildEditor.UndoIsAvailable)
                Redo();
            else
                Undo();
        });
    }

    private void Redo()
    {
        _buildEditor.Redo();
        _currentDelay = Tween.Delay(1f, () =>
        {
            if (!_buildEditor.RedoIsAvailable)
                Undo();
            else
                Redo();
        });
    }

    private void SpawnPiece()
    {
        var piece = _buildEditor.Build.Add(_buildTemplateSelector.SelectedTemplate);
        _pieceId = piece.GetTransientData().Id;
        piece.MoveTo(piece.GetSweepPosition(transform.position + Vector3.up, Vector3.down));
        piece.TrySetColor(_buildColorSelector.GetSelectedColorFor(0), 0);
        var command = new SpawnPieceCommand(_buildEditor.Build, piece.GetData());
        _buildEditor.Commit(command);
    }

    private void MovePiece()
    {
        var piece = _buildEditor.Build.GetPiece(_pieceId);
        if (piece == null)
            return;

        var moveTarget = _editablePieceTargetResolver.Resolve(piece);
        if (moveTarget == null)
            return;

        moveTarget.BeginMove(piece);

        var ray = new Ray(transform.position + new Vector3(0.8f * 4, 1), Vector3.down);
        if (moveTarget.TryGetMovePosition(ray, out var position))
            moveTarget.UpdateMove(position);

        var command = moveTarget.EndMove();
        if (command != null)
            _buildEditor.Commit(command);
    }

    private void PaintPiece()
    {
        var piece = _buildEditor.Build.GetPiece(_pieceId);
        var previousColor = new SimpleColor(Color.white);
        var newColor = new SimpleColor(Color.yellow);
        piece.TrySetColor(newColor, 0);
        var command = new PaintPiecesCommand(_buildEditor.Build,
            new Dictionary<Guid, PieceColor> { { _pieceId, previousColor } }, newColor);
        _buildEditor.Commit(command);
    }

    private void RemovePiece()
    {
        var piece = _buildEditor.Build.GetPiece(_pieceId);
        if (piece == null)
            return;
        var data = piece.GetData();
        _buildEditor.Build.Remove(piece);
        var command = new RemovePiecesCommand(_buildEditor.Build, new[] { data });
        _buildEditor.Commit(command);
    }
}
