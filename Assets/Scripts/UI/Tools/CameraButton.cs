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

    [SerializeField, Child(Flag.ExcludeSelf)]
    private Image _selectedImage;

    private void Awake()
    {
        gameObject.SetActive(false);
        _button.onClick.AddListener(OnClick);

        _toolController.ToolSelected += _ => _selectedImage.enabled = false;
        _toolController.ToolDeselected += _ => _selectedImage.enabled = true;
    }

    private void OnClick()
    {
        _toolController.DeselectTool();
    }
}
