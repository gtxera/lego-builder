using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraControlInputContext : InputContext
{
    private bool _primaryTouchPerformed;
    private bool _secondTouchPerformed;
    private Vector2 _firstTouchDelta;
    private Vector2 _secondTouchDelta;
    private bool _touchBeganInUI;

    private readonly PointerUIController _pointerUIController;
    
    public CameraControlInputContext(LegoBuilderInputActions inputActions, PointerUIController pointerUIController) : base(inputActions)
    {
        _pointerUIController = pointerUIController;
    }

    public event Action CameraMoveStarted = delegate { };
    public event Action<Vector2> CameraMoveRequested = delegate { };
    public event Action CameraMoveFinished = delegate { };
    public event Action<float> CameraLookOrbitYRequested = delegate { };
    public event Action<float> CameraLookOrbitXRequested = delegate { };
    public event Action<float> CameraZoomRequested = delegate { }; 

    protected override void Enable(LegoBuilderInputActions inputActions)
    {
        inputActions.Camera.FirstTouch.performed += OnFirstTouchPerformed;
        inputActions.Camera.FirstTouch.canceled += OnFirstTouchCanceled;

        inputActions.Camera.SecondTouch.performed += OnSecondTouchPerformed;
        inputActions.Camera.SecondTouch.canceled += OnSecondTouchCanceled;

        inputActions.Camera.Touch.performed += OnMoveStarted;
        inputActions.Camera.Move.performed += OnMovePerformed;
        inputActions.Camera.Touch.canceled += OnMoveCanceled;
        
        inputActions.Camera.Look.performed += OnLookPerformed;
        inputActions.Camera.Zoom.performed += OnZoomPerformed;
    }

    protected override void Disable(LegoBuilderInputActions inputActions)
    {
        inputActions.Camera.FirstTouch.performed -= OnFirstTouchPerformed;
        inputActions.Camera.FirstTouch.canceled -= OnFirstTouchCanceled;

        inputActions.Camera.SecondTouch.performed -= OnSecondTouchPerformed;
        inputActions.Camera.SecondTouch.canceled -= OnSecondTouchCanceled;

        inputActions.Camera.Touch.performed -= OnMoveStarted;
        inputActions.Camera.Move.performed -= OnMovePerformed;
        inputActions.Camera.Touch.canceled -= OnMoveCanceled;
        
        inputActions.Camera.Look.performed -= OnLookPerformed;
        inputActions.Camera.Zoom.performed -= OnZoomPerformed;
    }

    private void OnFirstTouchPerformed(InputAction.CallbackContext context)
    {
        if (!_secondTouchPerformed)
            HandleCameraMoveRequest(_firstTouchDelta);
    }

    private void OnFirstTouchCanceled(InputAction.CallbackContext _)
    {
        _primaryTouchPerformed = false;
    }

    private void OnSecondTouchPerformed(InputAction.CallbackContext context)
    {
        if (context.control is not TouchControl)
            throw new InvalidOperationException("Callback de touch precisa ter controle de touch");
        
        _secondTouchPerformed = true;
        
        if (!_primaryTouchPerformed)
            return;
    }

    private void OnSecondTouchCanceled(InputAction.CallbackContext _)
    {
        _secondTouchPerformed = false;
    }

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");

        _touchBeganInUI = _pointerUIController.IsPointerOverUI(pointer.position.ReadValue());

        if (!_touchBeganInUI)
            CameraMoveStarted();
    }
    
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (_touchBeganInUI)
            return;
        
        HandleCameraMoveRequest(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext _)
    {
        var touchBeganInUI = _touchBeganInUI;
        _touchBeganInUI = false;
        
        if (touchBeganInUI)
            return;

        CameraMoveFinished();
    }

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
