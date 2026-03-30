using System;
using System.Collections.Generic;
using System.Linq;

public class BuildCatalogService
{
    private readonly SavedPieceSetLibrary _savedPieceSetLibrary;
    private readonly Dictionary<BuildCatalogCategory, IBuildCatalogItem[]> _itemsByCategory = new();

    public BuildCatalogService(PieceTemplateDatabase pieceTemplateDatabase, SavedPieceSetLibrary savedPieceSetLibrary)
    {
        _savedPieceSetLibrary = savedPieceSetLibrary;

        _itemsByCategory[BuildCatalogCategory.Brick] = Wrap(pieceTemplateDatabase.GetTemplates<BrickPieceTemplate>(), BuildCatalogCategory.Brick);
        _itemsByCategory[BuildCatalogCategory.Plate] = Wrap(pieceTemplateDatabase.GetTemplates<PlatePieceTemplate>(), BuildCatalogCategory.Plate);
        _itemsByCategory[BuildCatalogCategory.Tile] = Wrap(pieceTemplateDatabase.GetTemplates<TilePieceTemplate>(), BuildCatalogCategory.Tile);
        _itemsByCategory[BuildCatalogCategory.Ramp] = Wrap(pieceTemplateDatabase.GetTemplates<RampPieceTemplate>(), BuildCatalogCategory.Ramp);
        _itemsByCategory[BuildCatalogCategory.Mesh] = Wrap(pieceTemplateDatabase.GetTemplates<MeshPieceTemplate>(), BuildCatalogCategory.Mesh);
    }

    public event Action SavedSetsChanged
    {
        add => _savedPieceSetLibrary.Changed += value;
        remove => _savedPieceSetLibrary.Changed -= value;
    }

    public IBuildCatalogItem GetDefaultItem()
    {
        return GetItems(BuildCatalogCategory.Brick).FirstOrDefault();
    }

    public IEnumerable<IBuildCatalogItem> GetItems(BuildCatalogCategory category)
    {
        if (category == BuildCatalogCategory.SavedSet)
            return _savedPieceSetLibrary.Sets.Select(definition => new SavedPieceSetCatalogItem(definition));

        return _itemsByCategory.TryGetValue(category, out var items) ? items : Array.Empty<IBuildCatalogItem>();
    }

    public bool ContainsSelection(string selectionId)
    {
        return !string.IsNullOrEmpty(selectionId) && GetAllItems().Any(item => item.SelectionId == selectionId);
    }

    public IBuildCatalogItem FindBySelectionId(string selectionId)
    {
        return GetAllItems().FirstOrDefault(item => item.SelectionId == selectionId);
    }

    private IEnumerable<IBuildCatalogItem> GetAllItems()
    {
        foreach (var items in _itemsByCategory.Values)
        {
            foreach (var item in items)
                yield return item;
        }

        foreach (var item in GetItems(BuildCatalogCategory.SavedSet))
            yield return item;
    }

    private static IBuildCatalogItem[] Wrap(IEnumerable<IPieceTemplate> templates, BuildCatalogCategory category)
    {
        return templates.Select(template => new TemplateCatalogItem(template, category)).Cast<IBuildCatalogItem>().ToArray();
    }
}
