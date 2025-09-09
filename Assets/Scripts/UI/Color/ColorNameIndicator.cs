using System;
using KBCore.Refs;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ColorNameIndicator : MonoBehaviour
{
    [SerializeField, Self]
    private TextMeshProUGUI _text;

    [SerializeField, Scene]
    private ColorSelector _colorSelector;

    private NamedColor _currentColor;
    
    private void Awake()
    {
        _colorSelector.ColorChanged += OnColorChanged;
    }

    private void OnColorChanged(Color color)
    {
        var newColor = new NamedColor(color);

        if (!newColor.Equals(_currentColor))
        {
            _currentColor = newColor;
            _text.SetText(_currentColor.ToString());
        }

        _text.color = color;
    }
}
