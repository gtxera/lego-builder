using System;
using System.Collections.Generic;
using UnityEngine;

public class PiecePreviewService
{
    private Vector3 _position = new Vector3(5000, 5000, 5000);

    private readonly Dictionary<Type, List<PiecePreview>> _previews = new();
    
    private const float FullRotationDuration = 5f;

    private readonly BuildColorSelector _colorSelector;
    
    public PiecePreviewService(BuildColorSelector colorSelector)
    {
        _colorSelector = colorSelector;
    }

    public Texture GetPreviewTexture(IPieceTemplate template, Vector2Int size)
    {
        var previewObject = new GameObject("Preview Piece");
        previewObject.transform.position = _position;
        var preview = previewObject.AddComponent<PiecePreview>();
        
        var type = template.GetType();
        if (!_previews.TryGetValue(type, out var previews))
        {
            previews = new List<PiecePreview>();
            _previews.Add(type, previews);
        }
        previews.Add(preview);
        
        return preview.GetRenderTexture(template, this, _colorSelector, size);
    }
    
    public void EnablePreview<TPieceTemplate>() where TPieceTemplate : IPieceTemplate
    {
        var type = typeof(TPieceTemplate);

        foreach (var (templateType, previews) in _previews)
        {
            if (templateType == type)
                foreach (var preview in previews)
                    preview.enabled = true;
            else
                foreach (var preview in previews)
                    preview.enabled = false;
        }
            
    }

    public float GetRotation(float currentTime) => currentTime % FullRotationDuration / FullRotationDuration * 360f;
}
