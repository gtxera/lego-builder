using UnityEngine;

public readonly struct ColorSelectorState
{
    public ColorSelectorState(float hue, float saturation, float value, bool transparent)
    {
        Hue = Mathf.Repeat(hue, 1f);
        Saturation = Mathf.Clamp01(saturation);
        Value = Mathf.Clamp01(value);
        Transparent = transparent;
    }

    public float Hue { get; }
    public float Saturation { get; }
    public float Value { get; }
    public bool Transparent { get; }
    public Color Color => Color.HSVToRGB(Hue, Saturation, Value);

    public static ColorSelectorState FromColor(Color color, bool transparent)
    {
        Color.RGBToHSV(color, out var hue, out var saturation, out var value);
        return new ColorSelectorState(hue, saturation, value, transparent);
    }
}
