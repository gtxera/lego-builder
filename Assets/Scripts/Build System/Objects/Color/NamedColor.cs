using System;
using UnityEngine;

public class NamedColor
{
    private readonly bool _isGray;

    private readonly Hue _hue;

    private readonly Shade _shade;

    private readonly GrayType _grayType;
    
    public NamedColor(Color color)
    {
        Color.RGBToHSV(color, out var hue, out var saturation, out var val);
        
        if (val < .2f)
        {
            _isGray = true;
            _grayType = GrayType.Black;
            return;
        }

        if (saturation < .2f)
        {
            _isGray = true;
            var grayIndex = Mathf.Clamp(Mathf.FloorToInt(val / .2f), 0, 4);
            _grayType = (GrayType)grayIndex;
            return;
        }

        var step = 1f / 12f;
        var hueIndex = Mathf.FloorToInt((hue + step / 2f) / step) % 12;
        _hue = (Hue)hueIndex;

        if (val < .6f)
        {
            _shade = saturation < .6f ? Shade.Dull : Shade.Dark;
        }
        else
        {
            _shade = saturation < .6f ? Shade.Pale : Shade.Bright;
        }
    }

    public override bool Equals(object obj)
    {
        return obj is NamedColor namedColor && BothAreSameColor(namedColor);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_isGray, _grayType, _hue, _shade);
    }

    private bool BothAreSameColor(NamedColor namedColor) =>
        BothAreSameGray(namedColor) || BothAreSameHueAndShare(namedColor);
    
    private bool BothAreSameGray(NamedColor namedColor) =>
        _isGray && namedColor._isGray && _grayType == namedColor._grayType;

    private bool BothAreSameHueAndShare(NamedColor namedColor) =>
        !_isGray && !namedColor._isGray && _hue == namedColor._hue && _shade == namedColor._shade;
    
    public override string ToString()
    {
        if (_isGray)
            return _grayType.ToString();
        else
            return $"{_hue} - {_shade}";
    }
}

public enum Hue
{
    Red,
    Orange,
    Yellow,
    Lime,
    Green,
    Mint,
    Cyan,
    Azure,
    Blue,
    Violet,
    Magenta,
    Rose,
}

public enum Shade
{
    Pale,
    Dull,
    Bright,
    Dark,
}

public enum GrayType
{
    Black,
    DarkGray,
    MidGray,
    LightGray,
    White,
}