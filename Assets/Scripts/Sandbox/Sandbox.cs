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
    
    void Start()
    {
        _buildEditor.StartEditing(_sandboxBuild);
        _cameraControlInputContext.Enable();
    }
}
