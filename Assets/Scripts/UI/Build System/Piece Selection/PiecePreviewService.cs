using System;
using System.Collections.Generic;
using UnityEngine;

public class PiecePreviewService
{
    private Vector3 _position = new Vector3(5000, 5000, 5000);

    private readonly Dictionary<string, PiecePreview> _previews = new();
    
    private const float FullRotationDuration = 5f;

    private readonly BuildColorSelector _colorSelector;
    
    public PiecePreviewService(BuildColorSelector colorSelector)
    {
        _colorSelector = colorSelector;
    }

    public Texture GetPreviewTexture(IBuildCatalogItem item, Vector2Int size)
    {
        if (item == null)
            return null;

        if (_previews.TryGetValue(item.SelectionId, out var cachedPreview))
            return cachedPreview.GetTexture();

        var previewObject = new GameObject("Preview Piece");
        previewObject.transform.position = _position;
        var preview = previewObject.AddComponent<PiecePreview>();

        _previews[item.SelectionId] = preview;
        return preview.GetRenderTexture(item, this, _colorSelector, size);
    }

    public void EnablePreview(BuildCatalogCategory category)
    {
        foreach (var preview in _previews.Values)
            preview.enabled = preview.Category == category;
    }

    public float GetRotation(float currentTime) => currentTime % FullRotationDuration / FullRotationDuration * 360f;
}
