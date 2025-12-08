using System.Drawing;

namespace NetDaemon.Lights.Extensions
{
    internal static class ReadOnlyCollectionExtensions
    {
        public static Color ToColor(this IReadOnlyCollection<double> rgbCollection)
        {
            return rgbCollection.Select(x => (int)x).ToArray().ToColor();
        }

        public static Color ToColor(this IReadOnlyCollection<int> rgbCollection)
        {
            if (rgbCollection.Count != 3)
            {
                throw new ArgumentException("The collection must contain exactly 3 elements representing R, G, and B.");
            }

            var r = rgbCollection.ElementAt(0);
            var g = rgbCollection.ElementAt(1);
            var b = rgbCollection.ElementAt(2);

            if (r is < 0 or > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(r), "R value must be between 0 and 255.");
            }

            if (g is < 0 or > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(g), "G value must be between 0 and 255.");
            }

            if (b is < 0 or > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(b), "B value must be between 0 and 255.");
            }

            return Color.FromArgb(r, g, b);
        }
    }
}
