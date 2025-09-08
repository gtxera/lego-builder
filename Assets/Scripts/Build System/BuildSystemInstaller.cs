using Reflex.Core;
using UnityEngine;

public class BuildSystemInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(BuildEditor));
        containerBuilder.AddScoped(typeof(BuildEditorCommandStack));
        containerBuilder.AddScoped(typeof(BuildColorSelector));
        containerBuilder.AddScoped(typeof(ToolController));
        containerBuilder.AddScoped(typeof(MoverTool));
        containerBuilder.AddScoped(typeof(SpawnerTool));
        containerBuilder.AddScoped(typeof(RemoverTool));
        containerBuilder.AddScoped(typeof(ScreenRaycaster));
        containerBuilder.AddScoped(typeof(PainterTool));
    }
}
