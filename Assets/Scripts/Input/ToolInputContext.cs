using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToolInputContext : InputContext
{
    private readonly PointerUIController _pointerUIController;

    private bool _pressBeganInUI;
    
    public ToolInputContext(LegoBuilderInputActions inputActions, PointerUIController pointerUIController) : base(inputActions)
    {
        _pointerUIController = pointerUIController;
    }

    public event Action<Vector2> Pressed = delegate { };
    public event Action<Vector2> Released = delegate { };
    public event Action<Vector2> Dragged = delegate { };
    public event Action Tapped = delegate { };

    protected override void Enable(LegoBuilderInputActions inputActions)
    {
        var press = inputActions.Tool.Press;
        
        press.performed += OnPressPerformed;
        press.canceled += OnPressCanceled;
        
        var drag = inputActions.Tool.Drag;
        
        drag.performed += OnDragPerformed;

        var secondaryTap = inputActions.Tool.SecondaryTap;

        secondaryTap.performed += OnSecondaryTapPerformed;
    }

    protected override void Disable(LegoBuilderInputActions inputActions)
    {
        var press = inputActions.Tool.Press;
        
        press.performed -= OnPressPerformed;
        press.canceled -= OnPressCanceled;
        
        var drag = inputActions.Tool.Drag;
        
        drag.performed -= OnDragPerformed;
        
        var secondaryTap = inputActions.Tool.SecondaryTap;

        secondaryTap.performed -= OnSecondaryTapPerformed;
    }

    private void OnPressPerformed(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");

        var pointerPosition = pointer.position.ReadValue();

        _pressBeganInUI = _pointerUIController.IsPointerOverUI(pointerPosition);
        
        if (_pressBeganInUI)
            return;
        
        Pressed(pointerPosition);
    }

    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");
        
        var pointerPosition = pointer.position.ReadValue();

        var pressBeganInUI = _pressBeganInUI;

        _pressBeganInUI = false;
        
        if (_pointerUIController.IsPointerOverUI(pointerPosition) || pressBeganInUI)
            return;
        
        Released(pointerPosition);
    }

    private void OnDragPerformed(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");
        
        var pointerPosition = pointer.position.ReadValue();
        
        if (_pointerUIController.IsPointerOverUI(pointerPosition) || _pressBeganInUI)
            return;
        
        Dragged(pointerPosition);
    }

    private void OnSecondaryTapPerformed(InputAction.CallbackContext context)
    {
        if (_pointerUIController.IsPointerOverUI(Pointer.current.position.ReadValue()))
            return;
        
        Tapped();
    }
}
