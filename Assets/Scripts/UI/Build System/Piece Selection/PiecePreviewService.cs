using UnityEngine;

public class PiecePreviewService
{
    private Vector3 _position = new Vector3(5000, 5000, 5000);

    private const float FullRotationDuration = 5f;

    private readonly BuildColorSelector _colorSelector;
    
    public PiecePreviewService(BuildColorSelector colorSelector)
    {
        _colorSelector = colorSelector;
    }

    public Texture GetPreviewTexture(IPieceTemplate template)
    {
        var previewObject = new GameObject("Preview Piece");
        previewObject.transform.position = _position;
        var preview = previewObject.AddComponent<PiecePreview>();
        return preview.GetRenderTexture(template, this, _colorSelector);
    }

    public float GetRotation(float currentTime) => currentTime % FullRotationDuration / FullRotationDuration * 360f;
}
