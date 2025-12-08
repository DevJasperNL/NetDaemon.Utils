using System.Drawing;

namespace NetDaemon.Lights.Extensions;

internal static class ColorExtensions
{
    public static IReadOnlyCollection<int> ToRgbCollection(this Color color)
        => new int[] { color.R, color.G, color.B }.AsReadOnly();

    public static Color BlendWithGammaCorrection(
        this Color color1, 
        Color color2, 
        double blendFactor, 
        double gamma = 2.8) // Note: I've tested this with my own Philips Hue lights and found that a gamma of 2.8 came closest to their native transition.
    {
        // Ensure the blend factor is between 0 and 1
        blendFactor = Math.Clamp(blendFactor, 0.0, 1.0);

        // Convert the RGB values to linear space
        var r1 = Math.Pow(color1.R / 255.0, gamma);
        var g1 = Math.Pow(color1.G / 255.0, gamma);
        var b1 = Math.Pow(color1.B / 255.0, gamma);

        var r2 = Math.Pow(color2.R / 255.0, gamma);
        var g2 = Math.Pow(color2.G / 255.0, gamma);
        var b2 = Math.Pow(color2.B / 255.0, gamma);

        // Perform the blend in linear space
        var r = r1 * (1 - blendFactor) + r2 * blendFactor;
        var g = g1 * (1 - blendFactor) + g2 * blendFactor;
        var b = b1 * (1 - blendFactor) + b2 * blendFactor;

        // Convert back to gamma space
        var blendedR = (int)(Math.Pow(r, 1 / gamma) * 255.0);
        var blendedG = (int)(Math.Pow(g, 1 / gamma) * 255.0);
        var blendedB = (int)(Math.Pow(b, 1 / gamma) * 255.0);

        // Clamp the result to 0-255 to ensure valid RGB values
        blendedR = Math.Clamp(blendedR, 0, 255);
        blendedG = Math.Clamp(blendedG, 0, 255);
        blendedB = Math.Clamp(blendedB, 0, 255);

        return Color.FromArgb(blendedR, blendedG, blendedB);
    }
}