using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToolController
{
    private readonly BuildInputContext _buildInputContext;
    private readonly CameraControlInputContext _cameraControlInputContext;
    private readonly BuildEditor _buildEditor;
    private readonly BuildSelection _buildSelection;
    private readonly EditablePieceTargetResolver _editablePieceTargetResolver;
    private readonly BuildTemplateSelector _buildTemplateSelector;
    private readonly BuildColorSelector _buildColorSelector;
    private readonly CameraServices _cameraServices;
    private readonly BuildActionMenu _buildActionMenu;

    private ITool _activeTool;
    private IEditablePieceTarget _activeMoveTarget;
    private Piece _activeReferencePiece;
    private bool _dragControlsCamera;
    private bool _selectionMoveArmed;

    public ToolController(
        BuildInputContext buildInputContext,
        CameraControlInputContext cameraControlInputContext,
        BuildEditor buildEditor,
        BuildSelection buildSelection,
        EditablePieceTargetResolver editablePieceTargetResolver,
        BuildTemplateSelector buildTemplateSelector,
        BuildColorSelector buildColorSelector,
        CameraServices cameraServices,
        BuildActionMenu buildActionMenu,
        BuildSelectionVisualizer buildSelectionVisualizer)
    {
        _buildInputContext = buildInputContext;
        _cameraControlInputContext = cameraControlInputContext;
        _buildEditor = buildEditor;
        _buildSelection = buildSelection;
        _editablePieceTargetResolver = editablePieceTargetResolver;
        _buildTemplateSelector = buildTemplateSelector;
        _buildColorSelector = buildColorSelector;
        _cameraServices = cameraServices;
        _buildActionMenu = buildActionMenu;
        _ = buildSelectionVisualizer;

        _buildEditor.StartedEditing += OnStartedEditing;
        _buildEditor.FinishedEditing += OnFinishedEditing;

        _buildInputContext.PieceTapped += OnPieceTapped;
        _buildInputContext.DragStarted += OnDragStarted;
        _buildInputContext.DragMoved += OnDragMoved;
        _buildInputContext.DragEnded += OnDragEnded;
        _buildInputContext.HoldTriggered += OnHoldTriggered;
        _buildInputContext.DoubleTapTriggered += OnDoubleTapTriggered;

        _buildActionMenu.ColorRequested += OnColorRequested;
        _buildActionMenu.MoveRequested += OnMoveRequested;
        _buildActionMenu.RemoveRequested += OnRemoveRequested;
    }

    public event Action SelectionMoveStarted = delegate { };
    public event Action SelectionMoveFinished = delegate { };
    public event Action CameraMoveStarted = delegate { };
    public event Action<Vector2> CameraMoveRequested = delegate { };
    public event Action CameraMoveFinished = delegate { };
    public event Action ActionMenuShown = delegate { };
    public event Action ActionMenuHidden = delegate { };

    public event Action ToolPressed = delegate { };
    public event Action ToolReleased = delegate { };
    public event Action<ITool> ToolSelected = delegate { };
    public event Action<ITool> ToolDeselected = delegate { };

    public void PickTool(ITool tool)
    {
        _activeTool = tool;
    }

    public void DeselectTool()
    {
        _activeTool = null;
    }

    private void OnStartedEditing(Build build)
    {
        _buildInputContext.Enable();
        _cameraControlInputContext.Enable();
        _cameraControlInputContext.DisableMoveControl();
    }

    private void OnFinishedEditing(Build build)
    {
        _buildInputContext.Disable();
        _cameraControlInputContext.EnableMoveControl();
        HideActionMenu();
        CancelCurrentMove();
        _dragControlsCamera = false;
        _selectionMoveArmed = false;
    }

    private void OnPieceTapped(Piece piece, Vector2 pointerScreenPosition)
    {
        if (piece == null || _buildEditor.Build == null)
            return;

        var previousSelection = _buildSelection.SelectedPieceIds.ToArray();
        var nextSelection = new HashSet<Guid>(previousSelection);

        if (!nextSelection.Remove(piece.Id))
            nextSelection.Add(piece.Id);

        if (!new HashSet<Guid>(previousSelection).SetEquals(nextSelection))
            _buildEditor.Commit(new SetSelectionCommand(_buildSelection, previousSelection, nextSelection));
    }

    private void OnDragStarted(Piece startPiece, Vector2 pointerScreenPosition)
    {
        HideActionMenu();

        if (_selectionMoveArmed && _buildSelection.HasSelection)
        {
            _selectionMoveArmed = false;
            if (TryBeginSelectionMove(startPiece))
                return;
        }

        if (startPiece != null)
        {
            EnsurePieceIncludedInSelection(startPiece);
            if (TryBeginSelectionMove(startPiece))
                return;
        }

        _dragControlsCamera = true;
        CameraMoveStarted();
    }

    private void OnDragMoved(Piece startPiece, Vector2 pointerScreenPosition, Vector2 pointerScreenDelta)
    {
        if (_activeMoveTarget != null && _activeReferencePiece != null)
        {
            UpdateSelectionMove(pointerScreenPosition);
            return;
        }

        if (_dragControlsCamera)
            CameraMoveRequested(NormalizeToScreen(pointerScreenDelta * 1000f));
    }

    private void OnDragEnded(Piece startPiece, Vector2 pointerScreenPosition)
    {
        if (_activeMoveTarget != null)
        {
            var command = _activeMoveTarget.EndMove();
            _activeMoveTarget = null;
            _activeReferencePiece = null;
            SelectionMoveFinished();

            if (command != null)
                _buildEditor.Commit(command);

            return;
        }

        if (_dragControlsCamera)
        {
            _dragControlsCamera = false;
            CameraMoveFinished();
        }
    }

    private void OnHoldTriggered(Piece piece, Vector2 pointerScreenPosition)
    {
        if (_buildEditor.Build == null)
            return;

        if (piece != null && !_buildSelection.Contains(piece))
            _buildEditor.Commit(new SetSelectionCommand(_buildSelection, _buildSelection.SelectedPieceIds, new[] { piece.Id }));

        _buildActionMenu.Show(pointerScreenPosition, _buildSelection.HasSelection);
        ActionMenuShown();
    }

    private void OnDoubleTapTriggered(Piece piece, Vector2 pointerScreenPosition)
    {
        HideActionMenu();
        SpawnPiece(pointerScreenPosition);
    }

    private void OnColorRequested()
    {
        HideActionMenu();

        var command = CreateEditableSelectionTarget()?.Paint(_buildColorSelector.GetSelectedColorFor(0));
        if (command != null)
            _buildEditor.Commit(command);
    }

    private void OnMoveRequested()
    {
        HideActionMenu();
        _selectionMoveArmed = _buildSelection.HasSelection;
    }

    private void OnRemoveRequested()
    {
        HideActionMenu();

        var command = CreateEditableSelectionTarget()?.Remove();
        if (command != null)
            _buildEditor.Commit(command);
    }

    private bool TryBeginSelectionMove(Piece startPiece)
    {
        var selectedPieces = _buildSelection.GetSelectedPieces(_buildEditor.Build);
        if (selectedPieces.Count > 1)
        {
            var referencePiece = selectedPieces.Contains(startPiece) ? startPiece : selectedPieces[0];
            return BeginMoveTarget(new SelectionTarget(_buildEditor.Build, _buildSelection, selectedPieces), referencePiece);
        }

        var fallbackReferencePiece = GetMoveReferencePiece(startPiece);
        if (fallbackReferencePiece == null)
            return false;

        return BeginMoveTarget(ResolveMoveTarget(fallbackReferencePiece), fallbackReferencePiece);
    }

    private void UpdateSelectionMove(Vector2 pointerScreenPosition)
    {
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);

        if (!_activeReferencePiece.TryGetAnchoredPosition(ray, out var position))
            position = _activeReferencePiece.GetSweepPosition(ray.origin, ray.direction);

        _activeMoveTarget.UpdateMove(position);
    }

    private void SpawnPiece(Vector2 pointerScreenPosition)
    {
        if (_buildEditor.Build == null)
            return;

        var piece = _buildEditor.Build.Add(_buildTemplateSelector.SelectedTemplate);
        piece.SetWorldRotation(0f);

        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        if (!piece.TryGetAnchoredPosition(ray, out var position))
            position = piece.GetSweepPosition(ray.origin, ray.direction);

        piece.MoveTo(position);
        piece.TrySetColor(_buildColorSelector.GetSelectedColorFor(0), 0);
        _buildEditor.Commit(new SpawnPieceCommand(_buildEditor.Build, piece.GetData()));
    }

    private void EnsurePieceIncludedInSelection(Piece piece)
    {
        if (_buildSelection.Contains(piece))
            return;

        var previousSelection = _buildSelection.SelectedPieceIds.ToArray();
        var nextSelection = new HashSet<Guid>(previousSelection) { piece.Id };
        _buildEditor.Commit(new SetSelectionCommand(_buildSelection, previousSelection, nextSelection));
    }

    private IEditablePieceTarget CreateEditableSelectionTarget()
    {
        if (_buildEditor.Build == null || !_buildSelection.HasSelection)
            return null;

        var selectedPieces = _buildSelection.GetSelectedPieces(_buildEditor.Build);
        if (selectedPieces.Count == 0)
            return null;

        return selectedPieces.Count == 1
            ? new SinglePieceTarget(_buildEditor.Build, selectedPieces[0], _buildSelection)
            : new SelectionTarget(_buildEditor.Build, _buildSelection, selectedPieces);
    }

    private IEditablePieceTarget ResolveMoveTarget(Piece referencePiece)
    {
        if (referencePiece == null)
            return null;

        if (_buildSelection.Contains(referencePiece))
        {
            var selectionTarget = CreateEditableSelectionTarget();
            if (selectionTarget != null)
                return selectionTarget;
        }

        return _editablePieceTargetResolver.Resolve(referencePiece);
    }

    private bool BeginMoveTarget(IEditablePieceTarget moveTarget, Piece referencePiece)
    {
        if (moveTarget == null || referencePiece == null)
            return false;

        _activeMoveTarget = moveTarget;
        _activeMoveTarget.BeginMove(referencePiece);
        _activeReferencePiece = _activeMoveTarget.ReferencePiece;
        if (_activeReferencePiece == null)
        {
            _activeMoveTarget = null;
            return false;
        }

        SelectionMoveStarted();
        return true;
    }

    private Piece GetMoveReferencePiece(Piece startPiece)
    {
        if (_buildEditor.Build == null || !_buildSelection.HasSelection)
            return null;

        if (startPiece != null && _buildSelection.Contains(startPiece))
            return startPiece;

        return _buildSelection.GetSelectedPieces(_buildEditor.Build).FirstOrDefault();
    }

    private void HideActionMenu()
    {
        if (!_buildActionMenu.IsVisible)
            return;

        _buildActionMenu.Hide();
        ActionMenuHidden();
    }

    private void CancelCurrentMove()
    {
        if (_activeMoveTarget == null)
            return;

        _activeMoveTarget.EndMove();
        _activeMoveTarget = null;
        _activeReferencePiece = null;
        SelectionMoveFinished();
    }

    private static Vector2 NormalizeToScreen(Vector2 vector)
    {
        var screenSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        return vector / screenSize;
    }
}
