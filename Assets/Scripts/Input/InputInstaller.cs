using Reflex.Core;
using UnityEngine;

public class InputInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var inputActions = new LegoBuilderInputActions();
        inputActions.Enable();
        containerBuilder.AddScoped(_ => inputActions, typeof(LegoBuilderInputActions));
        containerBuilder.AddScoped(typeof(CameraControlInputContext), typeof(CameraControlInputContext), typeof(ITickable));
        containerBuilder.AddScoped(typeof(ToolInputContext));
        containerBuilder.AddScoped(typeof(LevelSelectorInputContext));
        containerBuilder.AddScoped(typeof(TouchController));
        containerBuilder.AddSingleton(typeof(PointerUIController));
        
        
    }
}
