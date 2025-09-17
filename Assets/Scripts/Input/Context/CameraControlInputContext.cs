using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class CameraControlInputContext : InputContext
{
    private bool _primaryTouchPerformed;
    private bool _secondTouchPerformed;
    private Vector2 _firstTouchDelta;
    private Vector2 _secondTouchDelta;
    private bool _touchBeganInUI;

    private bool _moveControlEnabled = true;

    private readonly PointerUIController _pointerUIController;
    private readonly TouchController _touchController;
    
    public CameraControlInputContext(LegoBuilderInputActions inputActions, PointerUIController pointerUIController, TouchController touchController) : base(inputActions)
    {
        _pointerUIController = pointerUIController;
        _touchController = touchController;
    }

    public event Action CameraMoveStarted = delegate { };
    public event Action<Vector2> CameraMoveRequested = delegate { };
    public event Action CameraMoveFinished = delegate { };
    public event Action<float> CameraLookOrbitYRequested = delegate { };
    public event Action<float> CameraLookOrbitXRequested = delegate { };
    public event Action<float> CameraZoomRequested = delegate { };

    public void EnableMoveControl() => _moveControlEnabled = true;

    public void DisableMoveControl() => _moveControlEnabled = false;

    protected override void Enable(LegoBuilderInputActions inputActions)
    {
        _touchController.SingleTouchMoved += OnSingleTouchMoved;

        _touchController.TouchesAngleChanged += HandleCameraLookOrbitXRequested;
        _touchController.TouchesMagnitudeChanged += HandleCameraZoomRequested;
        _touchController.TouchesHeightChanged += HandleCameraLookOrbitYRequested;

        
        inputActions.Camera.Touch.performed += OnMoveStarted;
        inputActions.Camera.Move.performed += OnMovePerformed;
        inputActions.Camera.Touch.canceled += OnMoveCanceled;
        
        inputActions.Camera.Look.performed += OnLookPerformed;
        inputActions.Camera.Zoom.performed += OnZoomPerformed;
    }

    protected override void Disable(LegoBuilderInputActions inputActions)
    {
        inputActions.Camera.Touch.performed -= OnMoveStarted;
        inputActions.Camera.Move.performed -= OnMovePerformed;
        inputActions.Camera.Touch.canceled -= OnMoveCanceled;
        
        inputActions.Camera.Look.performed -= OnLookPerformed;
        inputActions.Camera.Zoom.performed -= OnZoomPerformed;
    }

    private void OnSingleTouchMoved(Vector2 delta)
    {
        if (!CanMove())
            return;
        
        HandleCameraMoveRequest(-delta);
    }

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");

        _touchBeganInUI = _pointerUIController.IsPointerOverUI(pointer.position.ReadValue());

        if (CanMove())
            CameraMoveStarted();
    }
    
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!CanMove())
            return;
        
        HandleCameraMoveRequest(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext _)
    {
        _touchBeganInUI = false;
        
        if (!CanMove())
            return;

        CameraMoveFinished();
    }

    private bool CanMove() => !_touchBeganInUI && _moveControlEnabled;

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");

        if (_touchBeganInUI)
            return;
        
        var delta = NormalizeToScreen(context.ReadValue<Vector2>());
        
        if (delta.x != 0)
            HandleCameraLookOrbitXRequested(delta.x);
        
        if (delta.y != 0)
            HandleCameraLookOrbitYRequested(delta.y);
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        if (_pointerUIController.IsPointerOverUI(Pointer.current.position.ReadValue()))
            return;
        
        var delta = context.ReadValue<Vector2>();
        
        HandleCameraZoomRequested(-delta.y);
    }

    private void HandleCameraMoveRequest(Vector2 delta)
    {
        CameraMoveRequested(NormalizeToScreen(delta));
    }

    private void HandleCameraLookOrbitYRequested(float delta)
    {
        CameraLookOrbitYRequested(delta);
    }

    private void HandleCameraLookOrbitXRequested(float delta)
    {
        CameraLookOrbitXRequested(delta);
    }

    private void HandleCameraZoomRequested(float delta)
    {
        CameraZoomRequested(delta);
    }

    private Vector2 NormalizeToScreen(Vector2 vector)
    {
        var screenSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        return vector / screenSize;
    }
}
