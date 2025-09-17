using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PiecePreview : MonoBehaviour
{
    private PiecePreviewService _piecePreviewService;

    private Transform _viewObject;

    private Camera _camera;

    private IEnumerable<Renderer> _renderers;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    public RenderTexture GetRenderTexture(
        IPieceTemplate template,
        PiecePreviewService piecePreviewService,
        BuildColorSelector colorSelector,
        Vector2Int size)
    {
        _piecePreviewService = piecePreviewService;
        
        var renderTexture = new RenderTexture(size.x, size.y, 24);

        _viewObject = new GameObject("View").transform;
        _viewObject.SetParent(transform, false);
        template.Configure(_viewObject.gameObject);

        var cameraObject = new GameObject("Camera");
        cameraObject.transform.SetParent(transform, false);
        cameraObject.transform.localPosition = Vector3.zero + template.GetSize().ToWorld() / 2 + Vector3.one * 1.5f;
        cameraObject.transform.LookAt(_viewObject);
        
        _camera = cameraObject.AddComponent<Camera>();
        _camera.enabled = false;
        _camera.targetTexture = renderTexture;
        _camera.cullingMask = LayerMask.GetMask("ExamplePieces");
        _camera.clearFlags = CameraClearFlags.Color;
        _camera.backgroundColor = Color.clear;

        _renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in _renderers)
            renderer.shadowCastingMode = ShadowCastingMode.Off;

        colorSelector.ColorChanged += OnSelectedColorChanged;
        
        return renderTexture;
    }

    private void OnSelectedColorChanged(Color color)
    {
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor(BaseColor, color);

        foreach (var renderer in _renderers)
            renderer.SetPropertyBlock(propertyBlock);
    }
    
    private void Update()
    {
        var rotation = _viewObject.eulerAngles;
        rotation.y = _piecePreviewService.GetRotation(Time.time);
        _viewObject.eulerAngles = rotation;
        
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
