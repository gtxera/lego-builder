using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToolController
{
    private const float DoubleTapRevertWindow = 0.3f;

    private readonly BuildInputContext _buildInputContext;
    private readonly CameraControlInputContext _cameraControlInputContext;
    private readonly BuildEditor _buildEditor;
    private readonly BuildSelection _buildSelection;
    private readonly EditablePieceTargetResolver _editablePieceTargetResolver;
    private readonly BuildTemplateSelector _buildTemplateSelector;
    private readonly BuildColorSelector _buildColorSelector;
    private readonly CameraServices _cameraServices;
    private readonly BuildActionMenu _buildActionMenu;
    private readonly PainterTool _painterTool;

    private ITool _activeTool;
    private IEditablePieceTarget _activeMoveTarget;
    private Piece _activeReferencePiece;
    private Piece _pendingSpawnPiece;
    private Vector2 _pendingSpawnPointerScreenPosition;
    private bool _pendingSpawnSelectionMoveActive;
    private bool _dragControlsCamera;
    private Guid _lastTappedPieceId;
    private Guid[] _lastTapPreviousSelection;
    private Guid[] _lastTapNextSelection;
    private float _lastTapTime;
    private bool _actionColorMenuActive;

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
        PainterTool painterTool,
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
        _painterTool = painterTool;
        _ = buildSelectionVisualizer;

        _buildEditor.StartedEditing += OnStartedEditing;
        _buildEditor.FinishedEditing += OnFinishedEditing;

        _buildInputContext.PieceTapped += OnPieceTapped;
        _buildInputContext.EmptyTapped += OnEmptyTapped;
        _buildInputContext.TapReleased += OnTapReleased;
        _buildInputContext.DragStarted += OnDragStarted;
        _buildInputContext.DragMoved += OnDragMoved;
        _buildInputContext.DragEnded += OnDragEnded;
        _buildInputContext.HoldTriggered += OnHoldTriggered;
        _buildInputContext.DoubleTapTriggered += OnDoubleTapTriggered;
        _buildInputContext.SecondaryTapTriggered += OnSecondaryTapTriggered;

        _cameraControlInputContext.CameraMoveStarted += OnCameraInteraction;
        _cameraControlInputContext.CameraMoveRequested += OnCameraMoveRequested;
        _cameraControlInputContext.CameraLookOrbitXRequested += OnCameraLookOrbitXRequested;
        _cameraControlInputContext.CameraLookOrbitYRequested += OnCameraLookOrbitYRequested;
        _cameraControlInputContext.CameraZoomRequested += OnCameraZoomRequested;

        _buildActionMenu.ColorRequested += OnColorRequested;
        _buildActionMenu.RotateRightRequested += OnRotateRightRequested;
        _buildActionMenu.RotateLeftRequested += OnRotateLeftRequested;
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
    public event Action<IReadOnlyList<Piece>> ColorSelectionRequested = delegate { };

    public void PickTool(ITool tool)
    {
        if (_activeTool == tool)
            return;

        if (tool != _painterTool)
            _actionColorMenuActive = false;

        if (_activeTool != null)
            ToolDeselected(_activeTool);

        _activeTool = tool;

        if (_activeTool != null)
            ToolSelected(_activeTool);
    }

    public void DeselectTool()
    {
        if (_activeTool == null)
            return;

        if (_activeTool == _painterTool)
            _actionColorMenuActive = false;

        ToolDeselected(_activeTool);
        _activeTool = null;
    }

    public bool StartExternalSpawnPlacement(IPieceTemplate template, Vector2 pointerScreenPosition)
    {
        return BeginPendingSpawnPlacement(template, pointerScreenPosition, true);
    }

    public void UpdateExternalSpawnPlacement(Vector2 pointerScreenPosition)
    {
        if (_pendingSpawnPiece == null)
            return;

        UpdatePendingSpawnPlacement(pointerScreenPosition);
    }

    public void FinishExternalSpawnPlacement()
    {
        if (_pendingSpawnPiece == null)
            return;

        FinalizePendingSpawnPlacement();
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
        CloseActionColorMenu();
        HideActionMenu();
        CancelPendingSpawnPlacement();
        CancelCurrentMove();
        _dragControlsCamera = false;
        ClearLastTapState();
    }

    private void OnPieceTapped(Piece piece, Vector2 pointerScreenPosition)
    {
        CloseActionColorMenu();
        HideActionMenu();

        if (piece == null || _buildEditor.Build == null)
            return;

        var previousSelection = _buildSelection.SelectedPieceIds.ToArray();
        var nextSelection = new HashSet<Guid>(previousSelection);

        if (!nextSelection.Remove(piece.Id))
            nextSelection.Add(piece.Id);

        if (!new HashSet<Guid>(previousSelection).SetEquals(nextSelection))
        {
            _buildEditor.Commit(new SetSelectionCommand(_buildSelection, previousSelection, nextSelection));
            RegisterImmediateTap(piece.Id, previousSelection, nextSelection);
        }
    }

    private void OnEmptyTapped(Vector2 pointerScreenPosition)
    {
        CloseActionColorMenu();
        HideActionMenu();
        ClearLastTapState();

        if (_buildEditor.Build == null || !_buildSelection.HasSelection)
            return;

        var previousSelection = _buildSelection.SelectedPieceIds.ToArray();
        _buildEditor.Commit(new SetSelectionCommand(_buildSelection, previousSelection, Array.Empty<Guid>()));
    }

    private void OnTapReleased(Vector2 pointerScreenPosition)
    {
        if (_pendingSpawnPiece != null)
        {
            UpdatePendingSpawnPlacement(pointerScreenPosition);
            FinalizePendingSpawnPlacement();
            return;
        }

        if (_buildActionMenu.IsVisible && _buildActionMenu.ContainsScreenPoint(pointerScreenPosition))
            return;

        HideActionMenu();
    }

    private void OnDragStarted(Piece startPiece, Vector2 pointerScreenPosition)
    {
        if (_pendingSpawnPiece != null)
        {
            BeginPendingSpawnSelectionMoveIfNeeded();
            UpdatePendingSpawnPlacement(pointerScreenPosition);
            return;
        }

        CloseActionColorMenu();
        HideActionMenu();
        ClearLastTapState();

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
        if (_pendingSpawnPiece != null)
        {
            UpdatePendingSpawnPlacement(pointerScreenPosition);
            return;
        }

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
        if (_pendingSpawnPiece != null)
        {
            FinalizePendingSpawnPlacement();
            return;
        }

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
        CloseActionColorMenu();
        ClearLastTapState();

        if (_buildEditor.Build == null)
            return;

        if (piece != null && !_buildSelection.Contains(piece))
            _buildEditor.Commit(new SetSelectionCommand(_buildSelection, _buildSelection.SelectedPieceIds, new[] { piece.Id }));

        _buildActionMenu.Show(pointerScreenPosition, _buildSelection.HasSelection);
        ActionMenuShown();
    }

    private void OnDoubleTapTriggered(Piece piece, Vector2 pointerScreenPosition)
    {
        CloseActionColorMenu();
        RevertImmediateTapIfNeeded(piece);
        BeginPendingSpawnPlacement(_buildTemplateSelector.SelectedTemplate, pointerScreenPosition, false);
    }

    private void OnSecondaryTapTriggered(Vector2 pointerScreenPosition)
    {
        if (_pendingSpawnPiece != null)
        {
            RotatePendingSpawnPlacement(pointerScreenPosition);
            return;
        }

        CloseActionColorMenu();

        if (_activeMoveTarget == null || _activeReferencePiece == null || !_activeMoveTarget.CanRotate)
            return;

        _activeMoveTarget.RotateClockwise();
        UpdateSelectionMove(pointerScreenPosition);
    }

    private void OnCameraInteraction()
    {
        CloseActionColorMenu();
        HideActionMenu();
    }

    private void OnCameraMoveRequested(Vector2 delta)
    {
        CloseActionColorMenu();
        HideActionMenu();
    }

    private void OnCameraLookOrbitXRequested(float delta)
    {
        CloseActionColorMenu();
        HideActionMenu();
    }

    private void OnCameraLookOrbitYRequested(float delta)
    {
        CloseActionColorMenu();
        HideActionMenu();
    }

    private void OnCameraZoomRequested(float delta)
    {
        CloseActionColorMenu();
        HideActionMenu();
    }

    private void OnColorRequested()
    {
        HideActionMenu();
        if (_buildEditor.Build == null || !_buildSelection.HasSelection)
            return;

        var selectedPieces = _buildSelection.GetSelectedPieces(_buildEditor.Build);
        if (selectedPieces.Count == 0)
            return;

        PickTool(_painterTool);
        _actionColorMenuActive = true;
        ColorSelectionRequested(selectedPieces);
    }

    private void OnRotateRightRequested()
    {
        CloseActionColorMenu();
        HideActionMenu();
        RotateSelection(RotateDirection.Clockwise);
    }

    private void OnRotateLeftRequested()
    {
        CloseActionColorMenu();
        HideActionMenu();
        RotateSelection(RotateDirection.CounterClockwise);
    }

    private void OnRemoveRequested()
    {
        CloseActionColorMenu();
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

    private bool BeginPendingSpawnPlacement(IPieceTemplate template, Vector2 pointerScreenPosition, bool notifySelectionMove)
    {
        if (_buildEditor.Build == null || template == null)
            return false;

        HideActionMenu();
        ClearLastTapState();
        CancelPendingSpawnPlacement();

        _buildTemplateSelector.SetTemplate(template);

        _pendingSpawnPiece = _buildEditor.Build.Add(template);
        _pendingSpawnPiece.SetWorldRotation(0f);
        _pendingSpawnPiece.TrySetColor(_buildColorSelector.GetSelectedColorFor(0), 0);
        _pendingSpawnPiece.BeginDragging();
        _pendingSpawnSelectionMoveActive = false;

        UpdatePendingSpawnPlacement(pointerScreenPosition);

        if (notifySelectionMove)
        {
            _pendingSpawnSelectionMoveActive = true;
            SelectionMoveStarted();
        }

        return true;
    }

    private void UpdatePendingSpawnPlacement(Vector2 pointerScreenPosition)
    {
        if (_pendingSpawnPiece == null)
            return;

        _pendingSpawnPointerScreenPosition = pointerScreenPosition;

        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        if (!_pendingSpawnPiece.TryGetAnchoredPosition(ray, out var position))
            position = _pendingSpawnPiece.GetSweepPosition(ray.origin, ray.direction);

        _pendingSpawnPiece.MoveTo(position);
    }

    private void RotatePendingSpawnPlacement(Vector2 pointerScreenPosition)
    {
        if (_pendingSpawnPiece == null)
            return;

        _pendingSpawnPointerScreenPosition = pointerScreenPosition;
        _pendingSpawnPiece.RotateClockwise();
        UpdatePendingSpawnPlacement(_pendingSpawnPointerScreenPosition);
    }

    private void FinalizePendingSpawnPlacement()
    {
        FinalizePendingSpawnPlacement(true);
    }

    private void FinalizePendingSpawnPlacement(bool notifySelectionMoveFinished)
    {
        if (_pendingSpawnPiece == null)
            return;

        _pendingSpawnPiece.EndDragging();
        var piece = _pendingSpawnPiece;
        _pendingSpawnPiece = null;
        _pendingSpawnPointerScreenPosition = Vector2.zero;
        _buildEditor.Commit(new SpawnPieceCommand(_buildEditor.Build, piece.GetData()));

        if (notifySelectionMoveFinished && _pendingSpawnSelectionMoveActive)
            SelectionMoveFinished();

        _pendingSpawnSelectionMoveActive = false;
    }

    private void CancelPendingSpawnPlacement()
    {
        if (_pendingSpawnPiece == null)
            return;

        _pendingSpawnPiece.EndDragging();
        _buildEditor.Build.Remove(_pendingSpawnPiece);
        _pendingSpawnPiece = null;
        _pendingSpawnPointerScreenPosition = Vector2.zero;

        if (_pendingSpawnSelectionMoveActive)
            SelectionMoveFinished();

        _pendingSpawnSelectionMoveActive = false;
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

    private void HideActionMenu()
    {
        if (!_buildActionMenu.IsVisible)
            return;

        _buildActionMenu.Hide();
        ActionMenuHidden();
    }

    private void CloseActionColorMenu()
    {
        if (!_actionColorMenuActive)
            return;

        _actionColorMenuActive = false;

        if (_activeTool == _painterTool)
            DeselectTool();
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

    private void RegisterImmediateTap(Guid pieceId, IEnumerable<Guid> previousSelection, IEnumerable<Guid> nextSelection)
    {
        _lastTappedPieceId = pieceId;
        _lastTapPreviousSelection = previousSelection.ToArray();
        _lastTapNextSelection = nextSelection.ToArray();
        _lastTapTime = Time.unscaledTime;
    }

    private void RevertImmediateTapIfNeeded(Piece piece)
    {
        if (piece == null ||
            _lastTapPreviousSelection == null ||
            _lastTapNextSelection == null ||
            piece.Id != _lastTappedPieceId ||
            Time.unscaledTime - _lastTapTime > DoubleTapRevertWindow)
        {
            ClearLastTapState();
            return;
        }

        var currentSelection = _buildSelection.SelectedPieceIds.ToArray();
        if (!new HashSet<Guid>(currentSelection).SetEquals(_lastTapNextSelection))
        {
            ClearLastTapState();
            return;
        }

        if (!_buildEditor.TryPeekLastCommand(out var lastCommand) || lastCommand is not SetSelectionCommand)
        {
            ClearLastTapState();
            return;
        }

        _buildSelection.ReplaceSelection(_lastTapPreviousSelection);
        _buildEditor.TryDiscardLastCommand(out _);
        ClearLastTapState();
    }

    private void ClearLastTapState()
    {
        _lastTappedPieceId = Guid.Empty;
        _lastTapPreviousSelection = null;
        _lastTapNextSelection = null;
        _lastTapTime = 0f;
    }

    private void BeginPendingSpawnSelectionMoveIfNeeded()
    {
        if (_pendingSpawnPiece == null || _pendingSpawnSelectionMoveActive)
            return;

        _pendingSpawnSelectionMoveActive = true;
        SelectionMoveStarted();
    }

    private void RotateSelection(RotateDirection rotateDirection)
    {
        var moveTarget = CreateEditableSelectionTarget();
        if (moveTarget == null || !moveTarget.CanRotate)
            return;

        var selectedPieces = _buildSelection.GetSelectedPieces(_buildEditor.Build);
        if (selectedPieces.Count == 0)
            return;

        var initialStates = CapturePieceStates(selectedPieces);

        switch (rotateDirection)
        {
            case RotateDirection.Clockwise:
                moveTarget.RotateClockwise();
                break;
            case RotateDirection.CounterClockwise:
                moveTarget.RotateCounterClockwise();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rotateDirection), rotateDirection, null);
        }

        var command = CreateTransformCommand(selectedPieces, initialStates);
        if (command != null)
            _buildEditor.Commit(command);
    }

    private static Dictionary<Guid, (Vector3 Position, PieceRotation Rotation)> CapturePieceStates(IEnumerable<Piece> pieces)
    {
        var states = new Dictionary<Guid, (Vector3 Position, PieceRotation Rotation)>();

        foreach (var piece in pieces)
            states[piece.Id] = (piece.transform.position, piece.Rotation);

        return states;
    }

    private TransformPiecesCommand CreateTransformCommand(
        IEnumerable<Piece> pieces,
        IReadOnlyDictionary<Guid, (Vector3 Position, PieceRotation Rotation)> initialStates)
    {
        var transformedPieces = new Dictionary<Guid, (Vector3 StartPosition, Vector3 FinalPosition, PieceRotation StartRotation, PieceRotation FinalRotation)>();

        foreach (var piece in pieces)
        {
            if (!initialStates.TryGetValue(piece.Id, out var initialState))
                continue;

            var finalPosition = piece.transform.position;
            var finalRotation = piece.Rotation;
            if (finalPosition == initialState.Position && finalRotation == initialState.Rotation)
                continue;

            transformedPieces[piece.Id] = (initialState.Position, finalPosition, initialState.Rotation, finalRotation);
        }

        return transformedPieces.Count == 0 ? null : new TransformPiecesCommand(_buildEditor.Build, transformedPieces);
    }

    private Piece GetMoveReferencePiece(Piece startPiece)
    {
        if (_buildEditor.Build == null || !_buildSelection.HasSelection)
            return null;

        if (startPiece != null && _buildSelection.Contains(startPiece))
            return startPiece;

        return _buildSelection.GetSelectedPieces(_buildEditor.Build).FirstOrDefault();
    }

    private enum RotateDirection
    {
        Clockwise,
        CounterClockwise
    }
}
