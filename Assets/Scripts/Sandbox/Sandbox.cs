using Reflex.Attributes;
using UnityEngine;

public class Sandbox : MonoBehaviour
{
    [Inject]
    private readonly BuildEditor _buildEditor;
    
    [Inject]
    private readonly CameraControlInputContext _cameraControlInputContext;
    
    [SerializeField]
    private Build _sandboxBuild;

    [SerializeField]
    private JsonBuild _jsonBuild;
    
    void Start()
    {
        if (_jsonBuild != null)
            _sandboxBuild.Create(_jsonBuild.GetBuildData());
        
        _buildEditor.StartEditing(_sandboxBuild);
        _cameraControlInputContext.Enable();
    }
}
