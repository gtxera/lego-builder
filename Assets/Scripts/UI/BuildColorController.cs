using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;

public class BuildColorController : MonoBehaviour
{
    [Inject]
    private readonly BuildColorSelector _buildColorSelector;

    [SerializeField, Scene]
    private ColorSelector _colorSelector;

    private void Awake()
    {
        _colorSelector.ColorChanged += OnColorChanged;
    }

    private void OnColorChanged(Color color)
    {
        _buildColorSelector.SetColor(color);
    }
}
