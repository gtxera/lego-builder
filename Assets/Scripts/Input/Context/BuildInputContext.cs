using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildInputContext : InputContext
{
    private const float DoubleTapMaxDelay = 0.3f;
    private const float DoubleTapMaxDistancePixels = 32f;

    private readonly PointerUIController _pointerUiController;
    private readonly CameraServices _cameraServices;
    private readonly BuildEditor _buildEditor;

    private readonly float _dragThresholdPixels = 12f;

    private bool _gestureActive;
    private bool _gestureStartedOverUi;
    private bool _dragStarted;
    private bool _holdTriggered;
    private bool _doubleTapTriggered;
    private bool _tapCandidateAvailable;

    private Vector2 _startScreenPosition;
    private Vector2 _currentScreenPosition;
    private Vector2 _lastTapScreenPosition;
    private Piece _startPiece;
    private Piece _currentPiece;
    private float _lastTapTime;

    public BuildInputContext(
        LegoBuilderInputActions inputActions,
        PointerUIController pointerUiController,
        CameraServices cameraServices,
        BuildEditor buildEditor) : base(inputActions)
    {
        _pointerUiController = pointerUiController;
        _cameraServices = cameraServices;
        _buildEditor = buildEditor;
    }

    public event Action<Piece, Vector2> PieceTapped = delegate { };
    public event Action<Piece, Vector2> DragStarted = delegate { };
    public event Action<Piece, Vector2, Vector2> DragMoved = delegate { };
    public event Action<Piece, Vector2> DragEnded = delegate { };
    public event Action<Piece, Vector2> HoldTriggered = delegate { };
    public event Action<Piece, Vector2> DoubleTapTriggered = delegate { };
    public event Action<Vector2> SecondaryTapTriggered = delegate { };
    public event Action<Vector2> TapReleased = delegate { };
    public event Action<Vector2> EmptyTapped = delegate { };

    protected override void Enable(LegoBuilderInputActions inputActions)
    {
        inputActions.Build.Touch.started += OnTouchStarted;
        inputActions.Build.Touch.canceled += OnTouchCanceled;
        inputActions.Build.Drag.performed += OnDragPerformed;
        inputActions.Build.Hold.performed += OnHoldPerformed;
        inputActions.Build.SecondaryTap.performed += OnSecondaryTapPerformed;
    }

    protected override void Disable(LegoBuilderInputActions inputActions)
    {
        inputActions.Build.Touch.started -= OnTouchStarted;
        inputActions.Build.Touch.canceled -= OnTouchCanceled;
        inputActions.Build.Drag.performed -= OnDragPerformed;
        inputActions.Build.Hold.performed -= OnHoldPerformed;
        inputActions.Build.SecondaryTap.performed -= OnSecondaryTapPerformed;
        ResetGestureState();
        ClearTapCandidate();
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        var pointerPosition = ReadPointerPosition(context);
        _gestureStartedOverUi = _pointerUiController.IsPointerOverUI(pointerPosition);

        _gestureActive = true;
        _dragStarted = false;
        _holdTriggered = false;
        _startScreenPosition = pointerPosition;
        _currentScreenPosition = pointerPosition;
        _startPiece = ResolvePiece(pointerPosition);
        _currentPiece = _startPiece;

        _doubleTapTriggered = !_gestureStartedOverUi && IsDoubleTap(pointerPosition);
        if (_doubleTapTriggered)
        {
            ClearTapCandidate();
            DoubleTapTriggered(_startPiece, pointerPosition);
        }
    }

    private void OnDragPerformed(InputAction.CallbackContext context)
    {
        if (!_gestureActive)
            return;

        var pointerPosition = ReadPointerPosition(context);
        var pointerDelta = context.ReadValue<Vector2>();
        _currentScreenPosition = pointerPosition;
        _currentPiece = ResolvePiece(pointerPosition);

        if (_gestureStartedOverUi || _holdTriggered)
            return;

        if (!_dragStarted && Vector2.Distance(_startScreenPosition, pointerPosition) >= _dragThresholdPixels)
        {
            _dragStarted = true;
            DragStarted(_startPiece, _startScreenPosition);
        }

        if (_dragStarted)
            DragMoved(_startPiece, pointerPosition, pointerDelta);
    }

    private void OnHoldPerformed(InputAction.CallbackContext context)
    {
        if (!_gestureActive || _gestureStartedOverUi || _dragStarted || _doubleTapTriggered)
            return;

        _holdTriggered = true;

        var pointerPosition = ReadPointerPosition(context);
        _currentScreenPosition = pointerPosition;
        _currentPiece = ResolvePiece(pointerPosition);
        HoldTriggered(_currentPiece ?? _startPiece, pointerPosition);
    }

    private void OnSecondaryTapPerformed(InputAction.CallbackContext context)
    {
        var pointerPosition = ReadPointerPosition(context);
        if (_pointerUiController.IsPointerOverUI(pointerPosition))
            return;

        SecondaryTapTriggered(pointerPosition);
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        if (!_gestureActive)
            return;

        var pointerPosition = ReadPointerPosition(context);
        _currentScreenPosition = pointerPosition;
        _currentPiece = ResolvePiece(pointerPosition);

        if (!_dragStarted && !_holdTriggered)
            TapReleased(pointerPosition);

        if (!_gestureStartedOverUi)
        {
            if (_dragStarted)
            {
                DragEnded(_startPiece, pointerPosition);
            }
            else if (!_holdTriggered && !_doubleTapTriggered)
            {
                if (_startPiece != null)
                    PieceTapped(_startPiece, pointerPosition);
                else
                    EmptyTapped(pointerPosition);
            }
        }

        if (_gestureStartedOverUi || _dragStarted || _holdTriggered || _doubleTapTriggered)
        {
            if (_gestureStartedOverUi || _dragStarted || _holdTriggered)
                ClearTapCandidate();
        }
        else
        {
            RegisterTapCandidate(pointerPosition);
        }

        ResetGestureState();
    }

    private Vector2 ReadPointerPosition(InputAction.CallbackContext context)
    {
        if (context.control.device is Pointer pointer)
            return pointer.position.ReadValue();

        return Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
    }

    private Piece ResolvePiece(Vector2 screenPosition)
    {
        if (_buildEditor.Build == null)
            return null;

        var ray = _cameraServices.ScreenToWorldRay(screenPosition);
        if (!Physics.Raycast(ray, out var hit))
            return null;

        var piece = hit.transform.GetComponentInParent<Piece>();
        return piece != null && _buildEditor.Build.IsPartOfBuild(piece) ? piece : null;
    }

    private void ResetGestureState()
    {
        _gestureActive = false;
        _gestureStartedOverUi = false;
        _dragStarted = false;
        _holdTriggered = false;
        _doubleTapTriggered = false;
        _startScreenPosition = Vector2.zero;
        _currentScreenPosition = Vector2.zero;
        _startPiece = null;
        _currentPiece = null;
    }

    private bool IsDoubleTap(Vector2 pointerPosition)
    {
        return _tapCandidateAvailable &&
               Time.unscaledTime - _lastTapTime <= DoubleTapMaxDelay &&
               Vector2.Distance(_lastTapScreenPosition, pointerPosition) <= DoubleTapMaxDistancePixels;
    }

    private void RegisterTapCandidate(Vector2 pointerPosition)
    {
        _tapCandidateAvailable = true;
        _lastTapTime = Time.unscaledTime;
        _lastTapScreenPosition = pointerPosition;
    }

    private void ClearTapCandidate()
    {
        _tapCandidateAvailable = false;
        _lastTapTime = 0f;
        _lastTapScreenPosition = Vector2.zero;
    }
}
