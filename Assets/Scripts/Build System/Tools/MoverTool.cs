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

        _movingPiece = hit.transform.GetComponentInParent<Piece>();
        
        if (_movingPiece == null)
            return;

        _pieceInitialPosition = _lastMovePosition = _movingPiece.transform.position;
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
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        var newMovePosition = _movingPiece.GetSweepPosition(ray.origin, ray.direction);
        if ((newMovePosition - _lastMovePosition).magnitude > 0.01f)
        {
            _lastMovePosition = _movingPiece.MoveTo(newMovePosition);
        }
    }

    public void Tap()
    {
        if (_movingPiece == null)
            return;
        
        _movingPiece.RotateClockwise();
    }

    public Sprite GetIcon() => Resources.Load<Sprite>("Icons/Mover");
}
