using Reflex.Core;
using UnityEngine;

public class LevelsInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(LevelController));
        containerBuilder.AddSingleton(typeof(ProgressManager));
    }
}
