using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem;

public class JsonBuildSaver : ValidatedMonoBehaviour
{
    [SerializeField]
    private Build _build;

    // Update is called once per frame
    private void Update()
    {
        if (Keyboard.current.kKey.wasReleasedThisFrame)
            JsonBuild.Create(_build.GetBuildData().GetCentered());
    }
}
