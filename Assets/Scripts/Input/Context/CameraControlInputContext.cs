using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControlInputContext : InputContext, ITickable
{
    private const float PointerDeltaScale = 1000f;

    private Vector2 _lastFirstTouchPosition;
    private Vector2 _lastSecondTouchPosition;
    private float _lastTouchesDistance;
    private Vector2 _lastTouchesDirection;
    private bool _touchBeganInUI;

    private int _touchCount;
    private bool _moveControlEnabled = true;

    private readonly PointerUIController _pointerUIController;

    private readonly InputAction _firstTouchInputAction;
    private readonly InputAction _secondTouchInputAction;
    
    public CameraControlInputContext(LegoBuilderInputActions inputActions, PointerUIController pointerUIController, TouchController touchController) : base(inputActions)
    {
        _pointerUIController = pointerUIController;
        _ = touchController;

        _firstTouchInputAction = inputActions.Camera.FirstTouch;
        _secondTouchInputAction = inputActions.Camera.SecondTouch;
    }

    public event Action CameraMoveStarted = delegate { };
    public event Action<Vector2> CameraMoveRequested = delegate { };
    public event Action CameraMoveFinished = delegate { };
    public event Action<float> CameraLookOrbitYRequested = delegate { };
    public event Action<float> CameraLookOrbitXRequested = delegate { };
    public event Action<float> CameraZoomRequested = delegate { };

    public void ResetState()
    {
        _lastFirstTouchPosition = Vector2.zero;
        _lastSecondTouchPosition = Vector2.zero;
        _lastTouchesDistance = 0f;
        _lastTouchesDirection = Vector2.zero;
        _touchBeganInUI = false;
        _touchCount = 0;
        _moveControlEnabled = true;
    }

    public void EnableMoveControl() => _moveControlEnabled = true;

    public void DisableMoveControl() => _moveControlEnabled = false;

    protected override void Enable(LegoBuilderInputActions inputActions)
    {
        ResetState();

        inputActions.Camera.FirstTouchContact.performed += OnFirstTouchContact;
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
        ResetState();
        _moveControlEnabled = false;

        inputActions.Camera.FirstTouchContact.performed -= OnFirstTouchContact;
        inputActions.Camera.FirstTouchContact.canceled -= OnFirstTouchLifted;

        inputActions.Camera.SecondTouchContact.performed -= OnSecondTouchContact;
        inputActions.Camera.SecondTouchContact.canceled -= OnSecondTouchLifted;

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
    }
    
    private void OnFirstTouchLifted(InputAction.CallbackContext context)
    {
        _touchBeganInUI = false;
        if (_touchCount > 0)
            _touchCount--; 
        _lastFirstTouchPosition = Vector2.zero;
    }

    private void OnSecondTouchContact(InputAction.CallbackContext context)
    {
        if (context.control.device is not Touchscreen touchscreen)
            throw new InvalidOperationException("Touch deve ser touch");
        
        var position = touchscreen.touches[1].position.ReadValue();
        _lastSecondTouchPosition = position;
        _touchCount++;
        _lastTouchesDistance = Vector2.Distance(NormalizeToScreen(_lastFirstTouchPosition), NormalizeToScreen(_lastSecondTouchPosition));
        _lastTouchesDirection = (_lastSecondTouchPosition - _lastFirstTouchPosition).normalized;
    }

    private void OnSecondTouchLifted(InputAction.CallbackContext context)
    {
        if (_touchCount > 0)
            _touchCount--;
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

        HandleCameraMoveRequest(context.ReadValue<Vector2>() * PointerDeltaScale);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _touchBeganInUI = false;

        if (!CanMove())
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
            HandleCameraLookOrbitXRequested(delta.x * 160000);
        
        if (delta.y != 0)
            HandleCameraLookOrbitYRequested(delta.y * 190000);
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        if (Pointer.current != null && _pointerUIController.IsPointerOverUI(Pointer.current.position.ReadValue()))
            return;
        
        var delta = context.ReadValue<Vector2>();
        
        HandleCameraZoomRequested(-delta.y * 1000);
    }

    private void HandleCameraLookOrbitYRequested(float delta)
    {
        CameraLookOrbitYRequested(delta);
    }

    private void HandleCameraMoveRequest(Vector2 delta)
    {
        CameraMoveRequested(NormalizeToScreen(delta));
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

    private bool CanMove() => !_touchBeganInUI && _moveControlEnabled && _touchCount <= 1;

    public void Tick(float deltaTime)
    {
        if (_touchCount == 1 && !_touchBeganInUI && _moveControlEnabled)
        {
            var singleTouchPosition = _firstTouchInputAction.ReadValue<Vector2>();
            var singleTouchDelta = singleTouchPosition - _lastFirstTouchPosition;

            if (singleTouchDelta != Vector2.zero)
                HandleCameraMoveRequest(singleTouchDelta * PointerDeltaScale);

            _lastFirstTouchPosition = singleTouchPosition;
            return;
        }

        if (_touchCount != 2 || _touchBeganInUI || !_moveControlEnabled)
            return;

        var firstTouchPosition = _firstTouchInputAction.ReadValue<Vector2>();
        var secondTouchPosition = _secondTouchInputAction.ReadValue<Vector2>();

        var touchesDistance = Vector2.Distance(NormalizeToScreen(firstTouchPosition), NormalizeToScreen(secondTouchPosition));
        var touchesDirection = (secondTouchPosition - firstTouchPosition).normalized;

        var distanceDelta = touchesDistance - _lastTouchesDistance;
        var angle = Vector2.SignedAngle(_lastTouchesDirection, touchesDirection);

        HandleCameraZoomRequested(-distanceDelta * 8000);
        HandleCameraLookOrbitXRequested(angle * 5000);

        var firstTouchDelta = NormalizeToScreen(firstTouchPosition - _lastFirstTouchPosition);
        var secondTouchDelta = NormalizeToScreen(secondTouchPosition - _lastSecondTouchPosition);

        var firstSign = Mathf.Sign(firstTouchDelta.y);
        var secondSign = Mathf.Sign(secondTouchDelta.y);

        if (firstSign == secondSign)
        {
            var absoluteFirstDelta = Mathf.Abs(firstTouchDelta.y);
            var absoluteSecondDelta = Mathf.Abs(secondTouchDelta.y);

            var delta = Mathf.Max(absoluteFirstDelta, absoluteSecondDelta) * firstSign;
            HandleCameraLookOrbitYRequested(delta * 200000);
        }

        _lastTouchesDistance = touchesDistance;
        _lastTouchesDirection = touchesDirection;
        _lastFirstTouchPosition = firstTouchPosition;
        _lastSecondTouchPosition = secondTouchPosition;
    }
}
