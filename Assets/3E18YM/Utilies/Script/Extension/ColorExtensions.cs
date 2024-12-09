using UnityEngine;

public static class ColorExtensions
{
    public static Color ReplaceColorSV(this Color color, Color colorFix)
    {
        float h, s, v;
        float h2, s2, v2;
        Color.RGBToHSV(color, out h, out s, out v);
        Color.RGBToHSV(colorFix, out h2, out s2, out v2);
        // Adjust brightness
        color = Color.HSVToRGB(h, s2, v2);
        return color;
    }

    public static Color ChangeColorHue(this Color color, float hue = 0)
    {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        color = Color.HSVToRGB(hue, s, v);
        return color;

    }
}