using System.Linq;

public class BuildTemplateSelector
{
    public BuildTemplateSelector(PieceTemplateDatabase pieceTemplateDatabase)
    {
        SelectedTemplate = pieceTemplateDatabase.GetTemplates<BrickPieceTemplate>().First();
    }
    
    public IPieceTemplate SelectedTemplate { get; private set; }

    public void SetTemplate(IPieceTemplate template)
    {
        SelectedTemplate = template;
    }
}
