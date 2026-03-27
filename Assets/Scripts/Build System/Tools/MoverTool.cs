using UnityEngine;

public class MoverTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly CameraServices _cameraServices;
    private readonly EditablePieceTargetResolver _editablePieceTargetResolver;

    private IEditablePieceTarget _movingTarget;
    private Piece _referencePiece;

    public MoverTool(BuildEditor buildEditor, EditablePieceTargetResolver editablePieceTargetResolver, CameraServices cameraServices)
    {
        _buildEditor = buildEditor;
        _editablePieceTargetResolver = editablePieceTargetResolver;
        _cameraServices = cameraServices;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        
        if (!Physics.Raycast(ray, out var hit))
            return;

        var piece = hit.transform.GetComponentInParent<Piece>();
        _movingTarget = _editablePieceTargetResolver.Resolve(piece);
        _movingTarget?.BeginMove(piece);
        _referencePiece = _movingTarget?.ReferencePiece;
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        if (_movingTarget == null)
            return;

        var command = _movingTarget.EndMove();
        _movingTarget = null;
        _referencePiece = null;
        
        if (command != null)
            _buildEditor.Commit(command);
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        if (_movingTarget == null || _referencePiece == null)
            return;
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        if (_movingTarget.TryGetMovePosition(ray, out var position))
            _movingTarget.UpdateMove(position);
    }

    public void Tap(Vector2 pointerScreenPosition)
    {
        if (_movingTarget == null || !_movingTarget.CanRotate || _referencePiece == null)
            return;
        
        _movingTarget.RotateClockwise();
        
        var ray = _cameraServices.ScreenToWorldRay(pointerScreenPosition);
        if (_movingTarget.TryGetMovePosition(ray, out var position))
            _movingTarget.UpdateMove(position);
    }

    public Sprite GetIcon() => Resources.Load<Sprite>("Icons/Mover");
}
