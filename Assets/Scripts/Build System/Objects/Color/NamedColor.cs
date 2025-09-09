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
            return GrayName(_grayType);
        else
            return $"{HueName(_hue)} {ShadeName(_shade)}";
    }

    private static string GrayName(GrayType grayType)
    {
        return grayType switch
        {
            GrayType.Black => "Preto",
            GrayType.DarkGray => "Cinza Escuro",
            GrayType.MidGray => "Cinza",
            GrayType.LightGray => "Cinza Claro",
            GrayType.White => "Branco",
            _ => throw new ArgumentOutOfRangeException(nameof(grayType), grayType, null)
        };
    }

    private static string HueName(Hue hue)
    {
        return hue switch
        {
            Hue.Red => "Vermelho",
            Hue.Orange => "Laranja",
            Hue.Yellow => "Amarelo",
            Hue.Lime => "Lima",
            Hue.Green => "Verde",
            Hue.Mint => "Menta",
            Hue.Cyan => "Ciano",
            Hue.Azure => "Azure",
            Hue.Blue => "Azul",
            Hue.Violet => "Violeta",
            Hue.Magenta => "Magenta",
            Hue.Rose => "Rosa",
            _ => throw new ArgumentOutOfRangeException(nameof(hue), hue, null)
        };
    }

    private static string ShadeName(Shade shade)
    {
        return shade switch
        {
            Shade.Pale => "Pálido",
            Shade.Dull => "Fosco",
            Shade.Bright => "Claro",
            Shade.Dark => "Escuro",
            _ => throw new ArgumentOutOfRangeException(nameof(shade), shade, null)
        };
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