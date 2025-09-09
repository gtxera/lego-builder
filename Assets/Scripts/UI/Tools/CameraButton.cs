using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CameraButton : MonoBehaviour
{
    [Inject]
    private ToolController _toolController;

    [SerializeField, Self]
    private Button _button;

    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _toolController.DeselectTool();
    }
}
