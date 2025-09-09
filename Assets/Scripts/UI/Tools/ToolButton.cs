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
    
    private ToolController _toolController;

    private ITool _tool;
    
    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
    }

    public void Initialize(ToolController toolController, ITool tool)
    {
        _toolController = toolController;
        _tool = tool;

        _toolController.ToolSelected += OnToolSelected;
        _toolController.ToolDeselected += OnToolDeselected;
    }

    private void OnToolSelected(ITool tool)
    {
        if (tool != _tool)
            return;
        
        _selectedBorder.color = Color.white;
    }

    private void OnToolDeselected(ITool tool)
    {
        if (tool != _tool)
            return;
        
        _selectedBorder.color = Color.clear;
    }

    private void OnClick()
    {
        _toolController.PickTool(_tool);
    }
}
