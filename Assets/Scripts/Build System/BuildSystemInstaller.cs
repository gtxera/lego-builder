using Reflex.Core;
using UnityEngine;

public class BuildSystemInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private PieceParticleEmitter _pieceParticleEmitter;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(BuildEditor))
            .AddScoped(typeof(BuildSelection))
            .AddScoped(typeof(BuildSelectionVisualizer))
            .AddScoped(typeof(BuildColorSelector))
            .AddScoped(typeof(BuildTemplateSelector))
            .AddScoped(typeof(EditablePieceTargetResolver))
            .AddScoped(typeof(SelectionRectangleOverlay))
            .AddSingleton(typeof(PieceTemplateDatabase))
            .AddScoped(typeof(CameraServices))
            .AddScoped(typeof(PiecePreviewService))
            .AddScoped(typeof(PiecePartsPool))
            .AddScoped(typeof(PieceResourceLoader<MeshPiece>))
            .AddScoped(typeof(PieceResourceLoader<PieceTag>))
            .AddScoped(_ => _pieceParticleEmitter, typeof(PieceParticleEmitter))
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

