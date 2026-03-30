using Reflex.Core;
using UnityEngine;

public class BuildSystemInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private PieceParticleEmitter _pieceParticleEmitter;

    [SerializeField]
    private PieceSelectionOutlineSettings _pieceSelectionOutline = new();
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(BuildEditor))
            .AddScoped(typeof(BuildSelection))
            .AddScoped(typeof(BuildSelectionVisualizer))
            .AddScoped(typeof(BuildColorSelector))
            .AddScoped(typeof(SavedPieceSetLibrary))
            .AddScoped(typeof(BuildCatalogService))
            .AddScoped(typeof(BuildTemplateSelector))
            .AddScoped(typeof(EditablePieceTargetResolver))
            .AddScoped(typeof(SelectionRectangleOverlay))
            .AddScoped(typeof(BuildActionMenu))
            .AddSingleton(typeof(PieceTemplateDatabase))
            .AddScoped(typeof(CameraServices))
            .AddScoped(typeof(PiecePreviewService))
            .AddScoped(typeof(PiecePartsPool))
            .AddScoped(typeof(PieceResourceLoader<MeshPiece>))
            .AddScoped(typeof(PieceResourceLoader<PieceTag>))
            .AddScoped(_ => _pieceParticleEmitter, typeof(PieceParticleEmitter))
            .AddScoped(_ => _pieceSelectionOutline, typeof(PieceSelectionOutlineSettings))
            .AddScoped(typeof(PieceMaterials));
        
        RegisterTools(containerBuilder);
    }

    private static void RegisterTools(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(ToolController))
            .AddScoped(typeof(SelectionTool), typeof(SelectionTool), typeof(ITool))
            .AddScoped(typeof(SpawnerTool), typeof(SpawnerTool), typeof(ITool))
            .AddScoped(typeof(MoverTool), typeof(MoverTool), typeof(ITool))
            .AddScoped(typeof(PainterTool), typeof(PainterTool), typeof(ITool))
            .AddScoped(typeof(RemoverTool), typeof(RemoverTool), typeof(ITool));
    }
}

[System.Serializable]
public class PieceSelectionOutlineSettings
{
    [SerializeField]
    private Outline.Mode _mode = Outline.Mode.OutlineVisible;

    [SerializeField]
    private Color _color = Color.green;

    [SerializeField, Range(0f, 10f)]
    private float _width = 4f;

    public Outline.Mode Mode => _mode;
    public Color Color => _color;
    public float Width => _width;
}
