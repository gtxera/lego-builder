using UnityEngine;

public class SpawnerTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly CameraServices _cameraServices;
    private readonly BuildColorSelector _buildColorSelector;
    private readonly BuildTemplateSelector _buildTemplateSelector;

    private Piece _newPiece;
    private Vector3 _lastMovePosition;
    private PieceRotation _rotation;

    public SpawnerTool(BuildEditor buildEditor, CameraServices cameraServices, BuildColorSelector buildColorSelector, BuildTemplateSelector buildTemplateSelector)
    {
        _buildEditor = buildEditor;
        _cameraServices = cameraServices;
        _buildColorSelector = buildColorSelector;
        _buildTemplateSelector = buildTemplateSelector;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        _newPiece = _buildEditor.Build.Add(_buildTemplateSelector.SelectedTemplate);
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        _newPiece.SetRotation(_rotation);
        
        if (!_newPiece.TryGetAnchoredPosition(ray, out var position))
            position = _newPiece.GetSweepPosition(ray.origin, ray.direction);
        
        _lastMovePosition = _newPiece.MoveTo(position);

        _newPiece.TrySetColor(_buildColorSelector.GetSelectedColorFor(0), 0);
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        var command = new SpawnPieceCommand(_buildEditor.Build, _newPiece.GetData());
        _newPiece = null;
        _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        if (_newPiece == null)
            return;
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!_newPiece.TryGetAnchoredPosition(ray, out var position))
            position = _newPiece.GetSweepPosition(ray.origin, ray.direction);
        
        _lastMovePosition = _newPiece.MoveTo(position);
    }

    public void Tap(Vector2 pointerScreenPosition)
    {
        if (_newPiece == null)
            return;

        _rotation = PieceRotationExtensions.Add(_rotation, PieceRotation.East);
        
        _newPiece.RotateClockwise();
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!_newPiece.TryGetAnchoredPosition(ray, out var position))
            position = _newPiece.GetSweepPosition(ray.origin, ray.direction);
        
        _lastMovePosition = _newPiece.MoveTo(position);
    }

    public Sprite GetIcon() => Resources.Load<Sprite>("Icons/Add");
}
