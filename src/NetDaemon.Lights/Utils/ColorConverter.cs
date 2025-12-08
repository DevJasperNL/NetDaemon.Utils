using System.Drawing;

namespace NetDaemon.Lights.Utils
{
    internal static class ColorConverter
    {
        public static Color KelvinToRgb(double kelvin)
        {
            var temp = kelvin / 100.0;
            double red, green, blue;

            // Calculate Red
            if (temp <= 66)
            {
                red = 255;
            }
            else
            {
                red = 329.698727446 * Math.Pow(temp - 60, -0.1332047592);
                red = Math.Clamp(red, 0, 255);
            }

            // Calculate Green
            if (temp <= 66)
            {
                green = 99.4708025861 * Math.Log(temp) - 161.1195681661;
            }
            else
            {
                green = 288.1221695283 * Math.Pow(temp - 60, -0.0755148492);
            }

            green = Math.Clamp(green, 0, 255);

            // Calculate Blue
            switch (temp)
            {
                case >= 66:
                    blue = 255;
                    break;
                case <= 19:
                    blue = 0;
                    break;
                default:
                    blue = 138.5177312231 * Math.Log(temp - 10) - 305.0447927307;
                    blue = Math.Clamp(blue, 0, 255);
                    break;
            }

            return Color.FromArgb((int)red, (int)green, (int)blue);
        }
    }
}
