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
    private bool _suppressEvents;

    public event Action<Color, bool> ColorChanged = delegate { };
    public event Action InteractionFinished = delegate { };

    public ColorSelectorState CurrentState => new(_hue, _saturation, _value, _transparentToggle.isOn);

    private void Awake()
    {
        _hueWheel.HueChanged += OnHueChanged;
        _hueWheel.InteractionFinished += OnInteractionFinished;
        _saturationValuePicker.SaturationValueChanged += OnSaturationValueChanged;
        _saturationValuePicker.InteractionFinished += OnInteractionFinished;
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
        if (_suppressEvents)
            return;

        CallColorChanged();
        OnInteractionFinished();
    }

    private void CallColorChanged()
    {
        if (_suppressEvents)
            return;

        var color = Color.HSVToRGB(_hue, _saturation, _value);
        ColorChanged(color, _transparentToggle.isOn);
    }

    private void OnInteractionFinished()
    {
        if (_suppressEvents)
            return;

        InteractionFinished();
    }

    public void SetState(ColorSelectorState state, bool notifyColorChanged = true)
    {
        _suppressEvents = true;

        _hue = state.Hue;
        _saturation = state.Saturation;
        _value = state.Value;

        _hueWheel.SetHue(_hue);
        _saturationValuePicker.SetHue(_hue);
        _saturationValuePicker.SetSaturationValue(new Vector2(_saturation, _value));
        _transparentToggle.isOn = state.Transparent;

        _suppressEvents = false;

        if (notifyColorChanged)
            ColorChanged(state.Color, state.Transparent);
    }
}
