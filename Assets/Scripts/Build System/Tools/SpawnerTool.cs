using UnityEngine;

public class SpawnerTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly CameraServices _cameraServices;
    private readonly BuildColorSelector _buildColorSelector;
    private readonly BuildTemplateSelector _buildTemplateSelector;

    private ICatalogItemPlacement _placement;

    public SpawnerTool(BuildEditor buildEditor, CameraServices cameraServices, BuildColorSelector buildColorSelector, BuildTemplateSelector buildTemplateSelector)
    {
        _buildEditor = buildEditor;
        _cameraServices = cameraServices;
        _buildColorSelector = buildColorSelector;
        _buildTemplateSelector = buildTemplateSelector;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        var selectedItem = _buildTemplateSelector.SelectedItem;
        if (selectedItem == null)
            return;

        _placement = selectedItem.CreatePlacement(_buildEditor.Build, _buildColorSelector.GetSelectedColorFor(0));
        _placement.UpdatePosition(_cameraServices.ScreenToWorldRay(pointerScreenPosition));
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        if (_placement == null)
            return;

        var command = _placement.Confirm();
        _placement = null;

        if (command != null)
            _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        if (_placement == null)
            return;

        _placement.UpdatePosition(_cameraServices.ScreenToWorldRay(pointerScreenPosition));
    }

    public void Tap(Vector2 pointerScreenPosition)
    {
        if (_placement == null)
            return;

        _placement.RotateClockwise();
        _placement.UpdatePosition(_cameraServices.ScreenToWorldRay(pointerScreenPosition));
    }

    public Sprite GetIcon() => Resources.Load<Sprite>("Icons/Add");
}
