using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelSelectorInputContext : InputContext
{
    private readonly PointerUIController _pointerUIController;

    public LevelSelectorInputContext(PointerUIController pointerUIController, LegoBuilderInputActions inputActions) : base(inputActions)
    {
        _pointerUIController = pointerUIController;
    }

    public event Action<Vector2> Tapped = delegate { };

    protected override void Enable(LegoBuilderInputActions inputActions)
    {
        inputActions.LevelSelector.Tap.performed += OnTap;
    }
    
    protected override void Disable(LegoBuilderInputActions inputActions)
    {
        inputActions.LevelSelector.Tap.performed -= OnTap;
    }

    private void OnTap(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            throw new InvalidOperationException("Press deve ser pointer");

        var pointerPosition = pointer.position.ReadValue();

        if (!_pointerUIController.IsPointerOverUI(pointerPosition))
            Tapped(pointerPosition);
    }
}
