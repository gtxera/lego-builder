using System;
using System.Linq;

public class BuildTemplateSelector
{
    public BuildTemplateSelector(PieceTemplateDatabase pieceTemplateDatabase)
    {
        SelectedTemplate = pieceTemplateDatabase.GetTemplates<BrickPieceTemplate>().First();
    }
    
    public IPieceTemplate SelectedTemplate { get; private set; }

    public event Action<IPieceTemplate> TemplateSelected = delegate { };
    public event Action<IPieceTemplate> TemplateDeselected = delegate { }; 
    
    public void SetTemplate(IPieceTemplate template)
    {
        if (SelectedTemplate == template)
            return;

        TemplateDeselected(SelectedTemplate);
        TemplateSelected(template);
        
        SelectedTemplate = template;
    }
}
