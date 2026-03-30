using System;
using Reflex.Core;
using UnityEngine;

public class InputInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var inputActions = new LegoBuilderInputActions();
        inputActions.Enable();
        containerBuilder.AddScoped(_ => inputActions, typeof(LegoBuilderInputActions));
        containerBuilder.AddScoped(typeof(CameraControlInputContext), typeof(CameraControlInputContext), typeof(ITickable), typeof(IDisposable));
        containerBuilder.AddScoped(typeof(BuildInputContext), typeof(BuildInputContext), typeof(IDisposable));
        containerBuilder.AddScoped(typeof(ToolInputContext), typeof(ToolInputContext), typeof(IDisposable));
        containerBuilder.AddScoped(typeof(LevelSelectorInputContext), typeof(LevelSelectorInputContext), typeof(IDisposable));
        containerBuilder.AddScoped(typeof(TouchController), typeof(TouchController), typeof(IDisposable));
        containerBuilder.AddSingleton(typeof(PointerUIController));
    }
}
