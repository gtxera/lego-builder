using UnityEngine;

public class MoverTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly CameraServices _cameraServices;

    private Piece _movingPiece;
    private Vector3 _pieceInitialPosition;
    private Vector3 _lastMovePosition;

    public MoverTool(BuildEditor buildEditor, CameraServices cameraServices)
    {
        _buildEditor = buildEditor;
        _cameraServices = cameraServices;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();

        if (piece == null || !_buildEditor.Build.IsPartOfBuild(piece))
            return;

        _movingPiece = piece;
        _pieceInitialPosition = _lastMovePosition = _movingPiece.transform.position;
        _movingPiece.BeginDragging();
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        if (_movingPiece == null)
            return;

        _movingPiece.EndDragging();
        
        var command = new MovePieceCommand(_buildEditor.Build, _movingPiece.Id, _pieceInitialPosition,
            _movingPiece.transform.position);

        _movingPiece = null;
        
        _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        if (_movingPiece == null)
            return;
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!_movingPiece.TryGetAnchoredPosition(ray, out var position))
            position = _movingPiece.GetSweepPosition(ray.origin, ray.direction);

        if (position == _lastMovePosition)
            return;

        _lastMovePosition = _movingPiece.MoveTo(position);
    }

    public void Tap(Vector2 pointerScreenPosition)
    {
        if (_movingPiece == null)
            return;
        
        _movingPiece.RotateClockwise();
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!_movingPiece.TryGetAnchoredPosition(ray, out var position))
            position = _movingPiece.GetSweepPosition(ray.origin, ray.direction);
        
        _lastMovePosition = _movingPiece.MoveTo(position);
    }

    public Sprite GetIcon() => Resources.Load<Sprite>("Icons/Mover");
}
