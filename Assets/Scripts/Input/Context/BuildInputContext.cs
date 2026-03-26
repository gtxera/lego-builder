using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildInputContext : InputContext, ITickable
{
    private const float DefaultTapDelay = 0.25f;

    private readonly PointerUIController _pointerUiController;
    private readonly CameraServices _cameraServices;
    private readonly BuildEditor _buildEditor;

    private readonly float _dragThresholdPixels = 12f;

    private bool _gestureActive;
    private bool _gestureStartedOverUi;
    private bool _dragStarted;
    private bool _holdTriggered;
    private bool _doubleTapTriggered;

    private Vector2 _startScreenPosition;
    private Vector2 _currentScreenPosition;
    private Piece _startPiece;
    private Piece _currentPiece;

    private PendingTap _pendingTap;

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

    protected override void Enable(LegoBuilderInputActions inputActions)
    {
        inputActions.Build.Touch.started += OnTouchStarted;
        inputActions.Build.Touch.canceled += OnTouchCanceled;
        inputActions.Build.Drag.performed += OnDragPerformed;
        inputActions.Build.Hold.performed += OnHoldPerformed;
        inputActions.Build.DoubleTap.performed += OnDoubleTapPerformed;
    }

    protected override void Disable(LegoBuilderInputActions inputActions)
    {
        inputActions.Build.Touch.started -= OnTouchStarted;
        inputActions.Build.Touch.canceled -= OnTouchCanceled;
        inputActions.Build.Drag.performed -= OnDragPerformed;
        inputActions.Build.Hold.performed -= OnHoldPerformed;
        inputActions.Build.DoubleTap.performed -= OnDoubleTapPerformed;
        ResetGestureState();
        _pendingTap = default;
    }

    public void Tick(float deltaTime)
    {
        if (_gestureActive || !_pendingTap.IsPending || Time.unscaledTime < _pendingTap.DispatchAt)
            return;

        var pendingTap = _pendingTap;
        _pendingTap = default;
        PieceTapped(pendingTap.Piece, pendingTap.ScreenPosition);
    }

    private void OnTouchStarted(InputAction.CallbackContext context)
    {
        var pointerPosition = ReadPointerPosition(context);
        _gestureStartedOverUi = _pointerUiController.IsPointerOverUI(pointerPosition);

        _gestureActive = true;
        _dragStarted = false;
        _holdTriggered = false;
        _doubleTapTriggered = false;
        _startScreenPosition = pointerPosition;
        _currentScreenPosition = pointerPosition;
        _startPiece = ResolvePiece(pointerPosition);
        _currentPiece = _startPiece;
    }

    private void OnDragPerformed(InputAction.CallbackContext context)
    {
        if (!_gestureActive)
            return;

        var pointerPosition = ReadPointerPosition(context);
        var pointerDelta = context.ReadValue<Vector2>();
        _currentScreenPosition = pointerPosition;
        _currentPiece = ResolvePiece(pointerPosition);

        if (_gestureStartedOverUi || _holdTriggered || _doubleTapTriggered)
            return;

        if (!_dragStarted && Vector2.Distance(_startScreenPosition, pointerPosition) >= _dragThresholdPixels)
        {
            _dragStarted = true;
            CancelPendingTap();
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
        CancelPendingTap();

        var pointerPosition = ReadPointerPosition(context);
        _currentScreenPosition = pointerPosition;
        _currentPiece = ResolvePiece(pointerPosition);
        HoldTriggered(_currentPiece ?? _startPiece, pointerPosition);
    }

    private void OnDoubleTapPerformed(InputAction.CallbackContext context)
    {
        var pointerPosition = ReadPointerPosition(context);
        if (_pointerUiController.IsPointerOverUI(pointerPosition))
            return;

        _doubleTapTriggered = true;
        CancelPendingTap();
        DoubleTapTriggered(ResolvePiece(pointerPosition), pointerPosition);
    }

    private void OnTouchCanceled(InputAction.CallbackContext context)
    {
        if (!_gestureActive)
            return;

        var pointerPosition = ReadPointerPosition(context);
        _currentScreenPosition = pointerPosition;
        _currentPiece = ResolvePiece(pointerPosition);

        if (!_gestureStartedOverUi)
        {
            if (_dragStarted)
            {
                DragEnded(_startPiece, pointerPosition);
            }
            else if (!_holdTriggered && !_doubleTapTriggered && _startPiece != null)
            {
                QueueTap(_startPiece, pointerPosition);
            }
        }

        ResetGestureState();
    }

    private void QueueTap(Piece piece, Vector2 screenPosition)
    {
        _pendingTap = new PendingTap(piece, screenPosition, Time.unscaledTime + DefaultTapDelay);
    }

    private void CancelPendingTap()
    {
        _pendingTap = default;
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

    private readonly struct PendingTap
    {
        public PendingTap(Piece piece, Vector2 screenPosition, float dispatchAt)
        {
            Piece = piece;
            ScreenPosition = screenPosition;
            DispatchAt = dispatchAt;
        }

        public Piece Piece { get; }
        public Vector2 ScreenPosition { get; }
        public float DispatchAt { get; }
        public bool IsPending => Piece != null;
    }
}
