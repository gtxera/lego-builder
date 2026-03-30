using UnityEngine;

public interface IBuildCatalogItem
{
    string SelectionId { get; }
    BuildCatalogCategory Category { get; }

    Bounds GetPreviewBounds();
    void ConfigurePreview(GameObject previewObject);
    void CleanupPreview(GameObject previewObject);
    ICatalogItemPlacement CreatePlacement(Build build, PieceColor color);
}
