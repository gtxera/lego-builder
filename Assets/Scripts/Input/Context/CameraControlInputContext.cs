using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraControlInputContext : InputContext, ITickable
{
    private bool _primaryTouchPerformed;
    private bool _secondTouchPerformed;
    private Vector2 _lastFirstTouchPosition;
    private Vector2 _lastSecondTouchPosition;
    private float _lastTouchesDistance;
    private Vector2 _lastTouchesDirection;
    private bool _touchBeganInUI;

    private int _touchCount;
    private bool _touchesProcessed;

    private bool _moveControlEnabled = true;

    private Tween _doubleTapDelay;
    private bool _doubleTapMode;

    private readonly PointerUIController _pointerUIController;
    private readonly TouchController _touchController;

    private readonly InputAction _firstTouchInputAction;
    private readonly InputAction _secondTouchInputAction;
    
    public CameraControlInputContext(LegoBuilderInputActions inputActions, PointerUIController pointerUIController, TouchController touchController) : base(inputActions)
    {
        _pointerUIController = pointerUIController;
        _touchController = touchController;

        _firstTouchInputAction = inputActions.Camera.FirstTouch;
        _secondTouchInputAction = inputActions.Camera.SecondTouch;
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
        inputActions.Camera.FirstTouchContact.performed += OnFirstTouchContact;
        inputActions.Camera.FirstTouch.performed += OnFirstTouchMoved;
        inputActions.Camera.FirstTouchContact.canceled += OnFirstTouchLifted;
        
        inputActions.Camera.SecondTouchContact.performed += OnSecondTouchContact;
        inputActions.Camera.SecondTouchContact.canceled += OnSecondTouchLifted;
        
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

    private void OnFirstTouchContact(InputAction.CallbackContext context)
    {
        if (context.control.device is not Touchscreen touchscreen)
            throw new InvalidOperationException("Touch deve ser touch");
        
        var position = touchscreen.touches[0].position.ReadValue();
        _touchBeganInUI = _pointerUIController.IsPointerOverUI(position);
        _touchCount++;
        _lastFirstTouchPosition = position;
        
        if (!_doubleTapDelay.isAlive)
            _doubleTapDelay = Tween.Delay(.25f);
        else
        {
            _doubleTapMode = true;
            _doubleTapDelay.Stop();
        }
    }

    private void OnFirstTouchMoved(InputAction.CallbackContext context)
    {
        if (_lastFirstTouchPosition == Vector2.zero)
            return;

        if (_doubleTapDelay.isAlive)
        {
            _doubleTapDelay.Stop();
            Debug.Log("moveu");
        }
        
        var position = context.ReadValue<Vector2>();
        var lastTouchPosition = _lastFirstTouchPosition;
        _lastFirstTouchPosition = position;
        
        if (!CanMove())
            return;
        
        var delta = lastTouchPosition - position;

        if (!_doubleTapMode)
            HandleCameraMoveRequest(delta * 10);
        else
        {
            var normalized = NormalizeToScreen(-delta);
            HandleCameraLookOrbitXRequested(normalized.x * 100000);
            HandleCameraLookOrbitYRequested(normalized.y * 100000);
        }
    }
    
    private void OnFirstTouchLifted(InputAction.CallbackContext context)
    {
        _touchBeganInUI = false;
        _touchCount--;
        _lastFirstTouchPosition = Vector2.zero;
        _doubleTapMode = false;
        Debug.Log($"toque 1 up");
    }

    private void OnSecondTouchContact(InputAction.CallbackContext context)
    {
        if (context.control.device is not Touchscreen touchscreen)
            throw new InvalidOperationException("Touch deve ser touch");
        
        var position = touchscreen.touches[1].position.ReadValue();

        Debug.Log($"toque 2 {position}");
        _lastSecondTouchPosition = position;
        _touchCount++;
        _lastTouchesDistance = Vector2.Distance(NormalizeToScreen(_lastFirstTouchPosition), NormalizeToScreen(_lastSecondTouchPosition));
        _lastTouchesDirection = (_lastSecondTouchPosition - _lastFirstTouchPosition).normalized;
    }

    private void OnSecondTouchLifted(InputAction.CallbackContext context)
    {
        _touchCount--;
        Debug.Log($"toque 2 up");
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

    private bool CanMove() => !_touchBeganInUI && _moveControlEnabled && _touchCount <= 1;

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");

        if (_touchBeganInUI)
            return;
        
        var delta = NormalizeToScreen(context.ReadValue<Vector2>());
        
        if (delta.x != 0)
            HandleCameraLookOrbitXRequested(delta.x * 1000);
        
        if (delta.y != 0)
            HandleCameraLookOrbitYRequested(delta.y * 1000);
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        if (_pointerUIController.IsPointerOverUI(Pointer.current.position.ReadValue()))
            return;
        
        var delta = context.ReadValue<Vector2>();
        
        HandleCameraZoomRequested(-delta.y * 1000);
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

    public void Tick(float deltaTime)
    {
        if (_touchCount != 2)
            return;

        Debug.Log("dois!");
        var firstTouchPosition = _firstTouchInputAction.ReadValue<Vector2>();
        var secondTouchPosition = _secondTouchInputAction.ReadValue<Vector2>();

        Debug.Log($"primeiro {firstTouchPosition}");
        Debug.Log($"segundo {secondTouchPosition}");
        
        var touchesDistance = Vector2.Distance(NormalizeToScreen(firstTouchPosition), NormalizeToScreen(secondTouchPosition));
        var touchesDirection = (secondTouchPosition - firstTouchPosition).normalized;

        var distanceDelta = touchesDistance - _lastTouchesDistance;
        var angle = Vector2.SignedAngle(_lastTouchesDirection, touchesDirection);
        
        HandleCameraZoomRequested(-distanceDelta * 500);
        HandleCameraLookOrbitXRequested(angle * 500);

        _lastTouchesDistance = touchesDistance;
        _lastTouchesDirection = touchesDirection;
    }
}
