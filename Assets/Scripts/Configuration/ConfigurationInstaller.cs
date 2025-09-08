using Reflex.Core;
using UnityEngine;

public class ConfigurationInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddTransient(typeof(SensitivitySettings));
    }
}
