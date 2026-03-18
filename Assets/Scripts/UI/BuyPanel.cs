using System;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class BuyPanel : MonoBehaviour
{
    [Inject]
    private readonly BuildEditor _buildEditor;

    [SerializeField]
    private RectTransform _panelRoot;

    [SerializeField]
    private RawImage _image;

    private Camera _camera;

    private void Awake()
    {
        var renderTexture = new RenderTexture(256, 256, 24);
        
        var cameraObject = new GameObject("Camera");
        _camera = cameraObject.AddComponent<Camera>();
        _camera.enabled = false;
        _camera.cullingMask = LayerMask.GetMask("ExamplePieces");
        _camera.clearFlags = CameraClearFlags.Color;
        _camera.backgroundColor = Color.clear;
        _camera.targetTexture = renderTexture;
        _camera.orthographic = true;
        
        _image.texture = renderTexture;
    }

    public void OpenPanel()
    {
        _panelRoot.gameObject.SetActive(true);
        
        var build = _buildEditor.Build;
        var buildTransform = build.transform;
        var cameraTransform = _camera.transform;
        cameraTransform.SetParent(buildTransform, false);
        var halfSize = build.GetBounds().max;
        halfSize.x = -halfSize.x;
        cameraTransform.localPosition = halfSize + Vector3.one * 1.5f;
        cameraTransform.LookAt(buildTransform);
        cameraTransform.SetParent(null);
        
        var bounds = build.GetBounds();
        
        FitOrtho(_camera, bounds);
        
        SetLayerRecursive(build.transform, LayerMask.NameToLayer("ExamplePieces"));
        _camera.Render();
        SetLayerRecursive(build.transform, 0);
    }
    
    public static void FitOrtho(Camera cam, Bounds bounds, float padding = 1.05f)
    {
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        Vector3[] corners =
        {
            c + new Vector3( e.x,  e.y,  e.z),
            c + new Vector3( e.x,  e.y, -e.z),
            c + new Vector3( e.x, -e.y,  e.z),
            c + new Vector3( e.x, -e.y, -e.z),
            c + new Vector3(-e.x,  e.y,  e.z),
            c + new Vector3(-e.x,  e.y, -e.z),
            c + new Vector3(-e.x, -e.y,  e.z),
            c + new Vector3(-e.x, -e.y, -e.z),
        };

        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        Matrix4x4 w2c = cam.transform.worldToLocalMatrix;

        foreach (var corner in corners)
        {
            Vector3 p = w2c.MultiplyPoint3x4(corner);
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;

        cam.orthographicSize =
            Mathf.Max(height * 0.5f, (width * 0.5f) / cam.aspect) * padding;
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
