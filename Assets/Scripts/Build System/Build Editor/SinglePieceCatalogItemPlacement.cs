using UnityEngine;

public class SinglePieceCatalogItemPlacement : ICatalogItemPlacement
{
    private readonly Build _build;
    private readonly Piece _piece;

    public SinglePieceCatalogItemPlacement(Build build, IPieceTemplate template, PieceColor color)
    {
        _build = build;
        _piece = _build.Add(template);
        _piece.SetWorldRotation(0f);

        for (var i = 0; i < _piece.Colors.Count; i++)
            _piece.TrySetColor(BuildColorSelector.Clone(color), i);

        _piece.BeginDragging();
    }

    public void UpdatePosition(Ray ray)
    {
        if (!_piece.TryGetAnchoredPosition(ray, out var position))
            position = _piece.GetSweepPosition(ray.origin, ray.direction);

        _piece.MoveTo(position);
    }

    public void RotateClockwise()
    {
        _piece.RotateClockwise();
    }

    public ICommand Confirm()
    {
        _piece.EndDragging();
        return new SpawnPieceCommand(_build, _piece.GetData());
    }

    public void Cancel()
    {
        _piece.EndDragging();
        _build.Remove(_piece);
    }
}
