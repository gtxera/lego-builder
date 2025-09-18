using Reflex.Core;
using UnityEngine;

public class BuildSystemInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private PieceParticleEmitter _pieceParticleEmitter;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(BuildEditor))
            .AddScoped(typeof(BuildColorSelector))
            .AddScoped(typeof(BuildTemplateSelector))
            .AddSingleton(typeof(PieceTemplateDatabase))
            .AddScoped(typeof(CameraServices))
            .AddScoped(typeof(PiecePreviewService))
            .AddScoped(typeof(PiecePartsPool))
            .AddScoped(_ => _pieceParticleEmitter, typeof(PieceParticleEmitter));
        
        RegisterTools(containerBuilder);
    }

    private static void RegisterTools(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(ToolController))
            .AddScoped(typeof(SpawnerTool), typeof(SpawnerTool), typeof(ITool))
            .AddScoped(typeof(MoverTool), typeof(MoverTool), typeof(ITool))
            .AddScoped(typeof(PainterTool), typeof(PainterTool), typeof(ITool))
            .AddScoped(typeof(RemoverTool), typeof(RemoverTool), typeof(ITool));
    }
}
