using UnityEngine;

public class SpawnerTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly ScreenRaycaster _screenRaycaster;
    private readonly BuildColorSelector _buildColorSelector;

    private Piece _newPiece;

    public SpawnerTool(BuildEditor buildEditor, ScreenRaycaster screenRaycaster, BuildColorSelector buildColorSelector)
    {
        _buildEditor = buildEditor;
        _screenRaycaster = screenRaycaster;
        _buildColorSelector = buildColorSelector;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        _newPiece = _buildEditor.Build.Add(new BrickPieceTemplate());
        var ray = _screenRaycaster.ScreenToWorldRay(pointerScreenPosition);
        var position = _newPiece.SweepMove(ray.origin, ray.direction);
        _newPiece.transform.position = position;

        _newPiece.TrySetColor(_buildColorSelector.GetSelectedColorFor(0), 0);
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        var command = new SpawnPieceCommand(_buildEditor.Build, _newPiece.GetData());
        
        _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        if (_newPiece == null)
            return;
        
        var ray = _screenRaycaster.ScreenToWorldRay(pointerScreenPosition);
        _newPiece.SweepMove(ray.origin, ray.direction);
    }

    public void Tap()
    {
        if (_newPiece == null)
            return;
        
        _newPiece.RotateClockwise();
    }
}
