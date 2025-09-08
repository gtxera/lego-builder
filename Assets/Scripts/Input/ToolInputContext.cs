using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToolInputContext : InputContext
{
    public ToolInputContext(LegoBuilderInputActions inputActions) : base(inputActions)
    {
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
        Pressed(pointerPosition);
    }

    private void OnPressCanceled(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");
        
        var pointerPosition = pointer.position.ReadValue();
        Released(pointerPosition);
    }

    private void OnDragPerformed(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");
        
        var pointerPosition = pointer.position.ReadValue();
        
        Dragged(pointerPosition);
    }

    private void OnSecondaryTapPerformed(InputAction.CallbackContext context)
    {
        Tapped();
    }
}
