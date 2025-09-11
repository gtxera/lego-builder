using Reflex.Core;
using UnityEngine;

public class BuildSystemInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(BuildEditor));
        containerBuilder.AddScoped(typeof(BuildEditorCommandStack));
        containerBuilder.AddScoped(typeof(BuildColorSelector));
        containerBuilder.AddScoped(typeof(CameraServices));
        
        RegisterTools(containerBuilder);
    }

    private static void RegisterTools(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(ToolController));
        containerBuilder.AddScoped(typeof(MoverTool), typeof(MoverTool), typeof(ITool));
        containerBuilder.AddScoped(typeof(SpawnerTool), typeof(SpawnerTool), typeof(ITool));
        containerBuilder.AddScoped(typeof(RemoverTool), typeof(RemoverTool), typeof(ITool));
        containerBuilder.AddScoped(typeof(PainterTool), typeof(PainterTool), typeof(ITool));
    }
}
