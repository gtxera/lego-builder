using UnityEngine;

public class SavedPieceSetCatalogItem : IBuildCatalogItem
{
    private readonly SavedPieceSetDefinition _definition;

    public SavedPieceSetCatalogItem(SavedPieceSetDefinition definition)
    {
        _definition = definition;
    }

    public string SelectionId => $"saved-set:{_definition.Id}";
    public BuildCatalogCategory Category => BuildCatalogCategory.SavedSet;
    public SavedPieceSetDefinition Definition => _definition;

    public Bounds GetPreviewBounds()
    {
        return _definition.BuildData.GetBounds();
    }

    public void ConfigurePreview(GameObject previewObject)
    {
        var buildObject = new GameObject("Saved Set Preview");
        buildObject.transform.SetParent(previewObject.transform, false);

        var build = buildObject.AddComponent<Build>();
        build.CreateLocal(_definition.BuildData);
    }

    public void CleanupPreview(GameObject previewObject)
    {
    }

    public ICatalogItemPlacement CreatePlacement(Build build, PieceColor color)
    {
        return new SavedPieceSetCatalogItemPlacement(build, _definition.BuildData, color);
    }
}
