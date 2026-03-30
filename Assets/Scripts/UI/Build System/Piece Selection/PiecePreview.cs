using System.Collections.Generic;
using Reflex.Exceptions;
using Reflex.Extensions;
using UnityEngine;
using UnityEngine.Rendering;

public class PiecePreview : MonoBehaviour
{
    private IBuildCatalogItem _catalogItem;
    private BuildColorSelector _colorSelector;
    private PieceMaterials _pieceMaterials;
    private RenderTexture _renderTexture;

    private Transform _viewObject;

    private Camera _camera;

    private IEnumerable<Renderer> _renderers;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    public RenderTexture GetRenderTexture(
        IBuildCatalogItem item,
        PiecePreviewService piecePreviewService,
        BuildColorSelector colorSelector,
        Vector2Int size)
    {
        _catalogItem = item;
        _colorSelector = colorSelector;
        _pieceMaterials = gameObject.scene.GetSceneContainer().Resolve<PieceMaterials>();
        
        _renderTexture = new RenderTexture(size.x, size.y, 24);

        _viewObject = new GameObject("View").transform;
        _viewObject.SetParent(transform, false);
        item.ConfigurePreview(_viewObject.gameObject);

        var bounds = item.GetPreviewBounds();

        var cameraObject = new GameObject("Camera");
        cameraObject.transform.SetParent(transform, false);
        cameraObject.transform.localPosition = bounds.center + bounds.extents + Vector3.one * 1.5f;
        cameraObject.transform.LookAt(bounds.center);
        
        _camera = cameraObject.AddComponent<Camera>();
        _camera.enabled = false;
        _camera.targetTexture = _renderTexture;
        _camera.cullingMask = LayerMask.GetMask("ExamplePieces");
        _camera.clearFlags = CameraClearFlags.Color;
        _camera.backgroundColor = Color.clear;

        _renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in _renderers)
            renderer.shadowCastingMode = ShadowCastingMode.Off;

        colorSelector.ColorChanged += OnSelectedColorChanged;
        OnSelectedColorChanged(colorSelector.GetSelectedColorFor(0));
        
        return _renderTexture;
    }

    public Texture GetTexture() => _renderTexture;
    public BuildCatalogCategory Category => _catalogItem?.Category ?? BuildCatalogCategory.Brick;

    private void OnDestroy()
    {
        try
        {
            if (_catalogItem != null && _viewObject != null)
                _catalogItem.CleanupPreview(_viewObject.gameObject);
        }
        catch (UnknownContractException) {}


        if (_colorSelector != null)
            _colorSelector.ColorChanged -= OnSelectedColorChanged;
    }

    private void OnSelectedColorChanged(PieceColor color)
    {
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor(BaseColor, color.Color);

        foreach (var renderer in _renderers)
        {
            renderer.sharedMaterial = _pieceMaterials.GetMaterial(color.Transparent);
            renderer.SetPropertyBlock(propertyBlock);
        }
        
        SetLayerRecursive(transform, LayerMask.NameToLayer("ExamplePieces"));
        _camera.Render();
        SetLayerRecursive(transform, 0);
    }

    private void SetLayerRecursive(Transform rootTransform, int layer)
    {
        foreach (Transform child in rootTransform)
        {
            child.gameObject.layer = layer;
            SetLayerRecursive(child, layer);
        }
    }
}
