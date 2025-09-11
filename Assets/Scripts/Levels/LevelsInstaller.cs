using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

public class LevelsInstaller : MonoBehaviour, IInstaller
{
    [Inject]
    private readonly LevelSelector _levelSelector;

    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(LevelController));
        containerBuilder.AddSingleton(typeof(ProgressManager));
        containerBuilder.AddScoped(typeof(LevelSelector));
    }
}
