using UnityEngine;

public class PiecePreview : MonoBehaviour
{
    private PiecePreviewService _piecePreviewService;

    private Transform _viewObject;

    private Camera _camera;

    private Renderer _renderer;
    
    public RenderTexture GetRenderTexture(IPieceTemplate template, PiecePreviewService piecePreviewService, BuildColorSelector colorSelector)
    {
        _piecePreviewService = piecePreviewService;
        
        var renderTexture = new RenderTexture(128, 128, 24);

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

        _renderer = GetComponentInChildren<Renderer>();

        colorSelector.ColorChanged += OnSelectedColorChanged;
        
        return renderTexture;
    }

    private void OnSelectedColorChanged(Color color)
    {
        _renderer.material.SetColor("_BaseColor", color);
    }
    
    private void Update()
    {
        var rotation = _viewObject.eulerAngles;
        rotation.y = _piecePreviewService.GetRotation(Time.time);
        _viewObject.eulerAngles = rotation;

        foreach (Transform child in _viewObject)
        {
            child.gameObject.layer = LayerMask.NameToLayer("ExamplePieces");

        }
        _camera.Render();
        foreach (Transform child in _viewObject)
        {
            child.gameObject.layer = 0;
        }
    }
}
