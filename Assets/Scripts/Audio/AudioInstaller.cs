using Reflex.Core;using UnityEngine;

public class AudioInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddSingleton(typeof(UiSfxPlayer));
    }
}