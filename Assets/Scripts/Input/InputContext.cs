using UnityEngine;

public abstract class InputContext
{
    private readonly LegoBuilderInputActions _inputActions;

    protected InputContext(LegoBuilderInputActions inputActions)
    {
        _inputActions = inputActions;
    }

    public void Enable()
    {
        Enable(_inputActions);
    }

    public void Disable()
    {
        Disable(_inputActions);
    }
    
    protected abstract void Enable(LegoBuilderInputActions inputActions);

    protected abstract void Disable(LegoBuilderInputActions inputActions);
}
