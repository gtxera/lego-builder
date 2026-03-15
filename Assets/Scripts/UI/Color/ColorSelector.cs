using KBCore.Refs;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    [SerializeField, Child]
    private HueWheel _hueWheel;

    [SerializeField, Child]
    private SaturationValuePicker _saturationValuePicker;

    [SerializeField, Child]
    private Toggle _transparentToggle;

    private float _hue;
    private float _saturation;
    private float _value;

    public event Action<Color, bool> ColorChanged = delegate { };

    private void Awake()
    {
        _hueWheel.HueChanged += OnHueChanged;
        _saturationValuePicker.SaturationValueChanged += OnSaturationValueChanged;
        _transparentToggle.onValueChanged.AddListener(OnTransparentToggleChanged);
    }

    private void OnHueChanged(float hue)
    {
        _hue = hue;
        _saturationValuePicker.SetHue(hue);
        CallColorChanged();
    }

    private void OnSaturationValueChanged(Vector2 saturationValue)
    {
        _saturation = saturationValue.x;
        _value = saturationValue.y;

        CallColorChanged();
    }

    private void OnTransparentToggleChanged(bool _)
    {
        CallColorChanged();
    }

    private void CallColorChanged()
    {
        var color = Color.HSVToRGB(_hue, _saturation, _value);
        ColorChanged(color, _transparentToggle.isOn);
    }
}
