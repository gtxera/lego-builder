using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToolButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    [SerializeField, Child(Flag.ExcludeSelf)]
    private Image _selectedBorder;

    [SerializeField, Child(Flag.Editable)]
    private Image _icon;
    
    private ToolController _toolController;

    private ITool _tool;
    
    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
        _icon.preserveAspect = true;
    }

    public void Initialize(ToolController toolController, ITool tool)
    {
        _toolController = toolController;
        _tool = tool;

        _toolController.ToolSelected += OnToolSelected;
        _toolController.ToolDeselected += OnToolDeselected;
        _icon.sprite = _tool.GetIcon();
    }

    private void OnToolSelected(ITool tool)
    {
        if (tool != _tool)
            return;
        
        _selectedBorder.gameObject.SetActive(true);
    }

    private void OnToolDeselected(ITool tool)
    {
        if (tool != _tool)
            return;
        
        _selectedBorder.gameObject.SetActive(false);
    }

    private void OnClick()
    {
        _toolController.PickTool(_tool);
    }
}
