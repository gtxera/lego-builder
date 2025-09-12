using UnityEngine;

public abstract class InputContext
{
    private readonly LegoBuilderInputActions _inputActions;

    private bool _enabled;

    protected InputContext(LegoBuilderInputActions inputActions)
    {
        _inputActions = inputActions;
    }

    public void Enable()
    {
        Debug.Log($"habilitou {GetType().Name}");
        if (_enabled)
            return;

        _enabled = true;
        Enable(_inputActions);
    }

    public void Disable()
    {
        if (!_enabled)
            return;

        _enabled = false;
        Disable(_inputActions);
    }
    
    protected abstract void Enable(LegoBuilderInputActions inputActions);

    protected abstract void Disable(LegoBuilderInputActions inputActions);
}
