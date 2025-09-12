using UnityEngine;

public class PiecePreviewService
{
    private Vector3 _position = new Vector3(5000, 5000, 5000);

    private const float FullRotationDuration = 5f;
    
    public PiecePreviewService()
    {
        
    }

    public Texture GetPreviewTexture(IPieceTemplate template)
    {
        var previewObject = new GameObject("Preview Piece");
        previewObject.transform.position = _position;
        var preview = previewObject.AddComponent<PiecePreview>();
        return preview.GetRenderTexture(template, this);
    }

    public float GetRotation(float currentTime) => currentTime % FullRotationDuration / FullRotationDuration * 360f;
}
