using System.Drawing;

namespace CodeCasa.NetDaemon.Lights.Extensions;

internal static class ColorExtensions
{
    /// <summary>
    /// Converts a <see cref="Color"/> to a read-only collection of its RGB component values.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A read-only collection containing the R, G, and B values in that order.</returns>
    public static IReadOnlyCollection<int> ToRgbCollection(this Color color)
        => new int[] { color.R, color.G, color.B }.AsReadOnly();
}