using UnityEngine;

public class TemplateCatalogItem : IBuildCatalogItem
{
    private readonly IPieceTemplate _template;
    private readonly BuildCatalogCategory _category;

    public TemplateCatalogItem(IPieceTemplate template, BuildCatalogCategory category)
    {
        _template = template;
        _category = category;
    }

    public string SelectionId => $"{_category}:{_template.GetType().FullName}:{JsonUtility.ToJson(_template)}";
    public BuildCatalogCategory Category => _category;
    public IPieceTemplate Template => _template;

    public Bounds GetPreviewBounds()
    {
        return new Bounds(Vector3.zero, _template.GetSize().ToWorld());
    }

    public void ConfigurePreview(GameObject previewObject)
    {
        _template.Configure(previewObject);
    }

    public void CleanupPreview(GameObject previewObject)
    {
        _template.OnDestroy(previewObject);
    }

    public ICatalogItemPlacement CreatePlacement(Build build, PieceColor color)
    {
        return new SinglePieceCatalogItemPlacement(build, _template, color);
    }
}
