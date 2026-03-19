using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionTool : ITool
{
    private readonly BuildEditor _buildEditor;
    private readonly BuildSelection _buildSelection;
    private readonly CameraServices _cameraServices;
    private readonly SelectionRectangleOverlay _selectionRectangleOverlay;

    private Vector2 _selectionStartScreenPosition;
    private Guid[] _selectionBeforeDrag = Array.Empty<Guid>();
    private bool _isSelecting;

    public SelectionTool(
        BuildEditor buildEditor,
        BuildSelection buildSelection,
        CameraServices cameraServices,
        SelectionRectangleOverlay selectionRectangleOverlay)
    {
        _buildEditor = buildEditor;
        _buildSelection = buildSelection;
        _cameraServices = cameraServices;
        _selectionRectangleOverlay = selectionRectangleOverlay;
    }

    public void Press(Vector2 pointerScreenPosition)
    {
        _selectionStartScreenPosition = pointerScreenPosition;
        _selectionBeforeDrag = _buildSelection.SelectedPieceIds.ToArray();
        _isSelecting = true;
        _selectionRectangleOverlay.Show(_selectionStartScreenPosition, pointerScreenPosition);
        UpdateSelection(pointerScreenPosition);
    }

    public void Release(Vector2 pointerScreenPosition)
    {
        if (!_isSelecting)
            return;

        _isSelecting = false;
        _selectionRectangleOverlay.Hide();
        UpdateSelection(pointerScreenPosition);

        var selectionAfterDrag = _buildSelection.SelectedPieceIds;
        if (new HashSet<System.Guid>(_selectionBeforeDrag).SetEquals(selectionAfterDrag))
            return;

        _buildEditor.Commit(new SetSelectionCommand(_buildSelection, _selectionBeforeDrag, selectionAfterDrag));
    }

    public void Drag(Vector2 pointerScreenPosition)
    {
        if (!_isSelecting)
            return;

        _selectionRectangleOverlay.Show(_selectionStartScreenPosition, pointerScreenPosition);
        UpdateSelection(pointerScreenPosition);
    }

    public void Tap(Vector2 pointerScreenPosition) { }

    public Sprite GetIcon() => Resources.Load<Sprite>("Icons/Mover");

    private static Rect GetScreenRect(Vector2 firstPoint, Vector2 secondPoint)
    {
        var min = Vector2.Min(firstPoint, secondPoint);
        var max = Vector2.Max(firstPoint, secondPoint);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void UpdateSelection(Vector2 pointerScreenPosition)
    {
        var selectionRect = GetScreenRect(_selectionStartScreenPosition, pointerScreenPosition);
        var selectedPieceIds = new List<System.Guid>();

        foreach (var piece in _buildEditor.Build.Pieces)
        {
            if (!_cameraServices.TryGetScreenRect(piece.GetWorldBounds(), out var pieceScreenRect))
                continue;

            if (!selectionRect.Overlaps(pieceScreenRect, true))
                continue;

            if (_cameraServices.IsPieceVisibleInScreenRect(piece, selectionRect))
                selectedPieceIds.Add(piece.Id);
        }

        _buildSelection.ReplaceSelection(selectedPieceIds);
    }
}
