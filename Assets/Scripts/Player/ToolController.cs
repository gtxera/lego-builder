using System;
using UnityEngine;

public class ToolController
{
    private readonly ToolInputContext _toolInputContext;
    private readonly CameraControlInputContext _cameraControlInputContext;

    private ITool _activeTool;

    public ToolController(ToolInputContext toolInputContext, CameraControlInputContext cameraControlInputContext, BuildEditor buildEditor)
    {
        _toolInputContext = toolInputContext;
        _cameraControlInputContext = cameraControlInputContext;

        buildEditor.FinishedEditing += _ => DeselectTool();
        
        _toolInputContext.Pressed += OnPressed;
        _toolInputContext.Released += OnReleased;
        _toolInputContext.Dragged += OnDrag;
        _toolInputContext.Tapped += OnTapped;
    }

    public event Action ToolPressed = delegate { };
    public event Action ToolReleased = delegate { };
    public event Action<ITool> ToolSelected = delegate { };
    public event Action<ITool> ToolDeselected = delegate { };

    public void PickTool(ITool tool)
    {
        if (_activeTool != tool)
            ToolDeselected(_activeTool);
        
        _activeTool = tool;
        ToolSelected(_activeTool);
        
        _toolInputContext.Enable();
        _cameraControlInputContext.Disable();
    }

    public void DeselectTool()
    {
        ToolDeselected(_activeTool);
        _activeTool = null;
        
        _cameraControlInputContext.Enable();
        _toolInputContext.Disable();
    }

    private void OnPressed(Vector2 pointerScreenPosition)
    {
        if (_activeTool == null)
            return;
        
        _activeTool.Press(pointerScreenPosition);
        ToolPressed();
    }

    private void OnReleased(Vector2 pointerScreenPosition)
    {
        if (_activeTool == null)
            return;
        
        _activeTool.Release(pointerScreenPosition);
        ToolReleased();
    }

    private void OnDrag(Vector2 pointerScreenDelta) => _activeTool?.Drag(pointerScreenDelta);

    private void OnTapped() => _activeTool?.Tap();
}
