using UnityEngine;

public class MoverTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly ScreenRaycaster _screenRaycaster;

    private Piece _movingPiece;
    private Vector3 _pieceInitialPosition;

    public MoverTool(BuildEditor buildEditor, ScreenRaycaster screenRaycaster)
    {
        _buildEditor = buildEditor;
        _screenRaycaster = screenRaycaster;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        var ray = _screenRaycaster.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit))
            return;

        _movingPiece = hit.transform.GetComponentInParent<Piece>();
        
        if (_movingPiece == null)
            return;

        _pieceInitialPosition = _movingPiece.transform.position;
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        if (_movingPiece == null)
            return;
        
        var command = new MovePieceCommand(_buildEditor.Build, _movingPiece.Id, _pieceInitialPosition,
            _movingPiece.transform.position);
        
        _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        if (_movingPiece == null)
            return;
        
        var ray = _screenRaycaster.ScreenToWorldRay(pointerScreenPosition);
        _movingPiece.SweepMove(ray.origin, ray.direction);
    }

    public void Tap()
    {
        if (_movingPiece == null)
            return;
        
        _movingPiece.RotateClockwise();
    }
}
