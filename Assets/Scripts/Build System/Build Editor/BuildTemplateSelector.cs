using System;
public class BuildTemplateSelector
{
    public BuildTemplateSelector(BuildCatalogService buildCatalogService)
    {
        SelectedItem = buildCatalogService.GetDefaultItem();
    }

    public IBuildCatalogItem SelectedItem { get; private set; }

    public event Action<IBuildCatalogItem> ItemSelected = delegate { };
    public event Action<IBuildCatalogItem> ItemDeselected = delegate { };

    public bool IsSelected(IBuildCatalogItem item)
    {
        return item != null &&
               SelectedItem != null &&
               item.SelectionId == SelectedItem.SelectionId;
    }

    public void SetItem(IBuildCatalogItem item)
    {
        if (item == null || IsSelected(item))
            return;

        var previous = SelectedItem;
        SelectedItem = item;

        ItemDeselected(previous);
        ItemSelected(item);
    }
}
