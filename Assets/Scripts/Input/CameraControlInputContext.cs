using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraControlInputContext : InputContext
{
    public CameraControlInputContext(LegoBuilderInputActions inputActions) : base(inputActions)
    {
    }
    
    private bool _primaryTouchPerformed;
    private bool _secondTouchPerformed;
    private Vector2 _firstTouchDelta;
    private Vector2 _secondTouchDelta;

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

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        HandleCameraMoveRequest(context.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext _) => CameraMoveFinished();

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        var delta = NormalizeToScreen(context.ReadValue<Vector2>());
        
        if (delta.x != 0)
            HandleCameraLookOrbitXRequested(delta.x);
        
        if (delta.y != 0)
            HandleCameraLookOrbitYRequested(delta.y);
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
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
