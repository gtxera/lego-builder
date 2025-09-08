using UnityEngine;

public enum DefinedColor
{
    White,
    Black,
    Gray
}

public static class ColorExtensions
{
    public static DefinedColor ToDefinedColor(this Color color)
    {
        Color.RGBToHSV(color, out var hue, out var saturation, out var value);

        return DefinedColor.White;
    }
}