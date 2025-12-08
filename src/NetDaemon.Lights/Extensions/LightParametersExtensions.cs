using System.Drawing;
using ColorConverter = NetDaemon.Lights.Utils.ColorConverter;

namespace NetDaemon.Lights.Extensions;

public static class LightParametersExtensions
{
    /// <summary>
    /// Converts a <see cref="LightParameters"/> object to a <see cref="LightTransition"/> with no transition time (immediate).
    /// </summary>
    /// <param name="lightParameters">The light parameters to convert.</param>
    /// <returns>A <see cref="LightTransition"/> with the specified parameters and no transition time.</returns>
    public static LightTransition AsTransition(this LightParameters lightParameters)
    {
        return new LightTransition
        {
            LightParameters = lightParameters
        };
    }

    /// <summary>
    /// Converts a <see cref="LightParameters"/> object to a <see cref="LightTransition"/> with a specified transition time.
    /// </summary>
    /// <param name="lightParameters">The light parameters to convert.</param>
    /// <param name="transitionTime">The duration of the transition.</param>
    /// <returns>A <see cref="LightTransition"/> with the specified parameters and transition time.</returns>
    public static LightTransition AsTransition(this LightParameters lightParameters, TimeSpan transitionTime)
    {
        return new LightTransition
        {
            LightParameters = lightParameters,
            TransitionTime = transitionTime
        };
    }

    /// <summary>
    /// Converts a <see cref="LightParameters"/> object to a <see cref="LightTransition"/> with a transition time specified in seconds.
    /// </summary>
    /// <param name="lightParameters">The light parameters to convert.</param>
    /// <param name="transitionTimeInSeconds">The duration of the transition in seconds.</param>
    /// <returns>A <see cref="LightTransition"/> with the specified parameters and transition time.</returns>
    public static LightTransition AsTransitionInSeconds(this LightParameters lightParameters, int transitionTimeInSeconds)
    {
        return new LightTransition
        {
            LightParameters = lightParameters,
            TransitionTime = TimeSpan.FromSeconds(transitionTimeInSeconds)
        };
    }

    /// <summary>
    /// Interpolates between two <see cref="LightParameters"/> objects to create an intermediate light state.
    /// Uses gamma-corrected blending for RGB colors and linear interpolation for color temperature and brightness.
    /// </summary>
    /// <param name="fromLightParameters">The starting light parameters.</param>
    /// <param name="toLightParameters">The ending light parameters.</param>
    /// <param name="progress">The interpolation progress between 0 and 1, where 0 returns fromLightParameters and 1 returns toLightParameters.</param>
    /// <returns>A new <see cref="LightParameters"/> object representing the interpolated light state.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the parameters cannot be interpolated due to missing color or color temperature values.</exception>
    public static LightParameters Interpolate(this LightParameters fromLightParameters,
            LightParameters toLightParameters, double progress)
    {
        if (fromLightParameters.Brightness == 0 && toLightParameters.Brightness == 0)
        {
            return LightParameters.Off();
        }

        // If one of the parameters have a brightness of 0 and no color values, we copy the color values from the other parameters.
        if (fromLightParameters.Brightness == 0)
        {
            if (fromLightParameters.ColorTemp == null && fromLightParameters.RgbColor == null)
            {
                fromLightParameters = toLightParameters with { Brightness = 0 };
            }
        }
        else if (toLightParameters.Brightness == 0)
        {
            if (toLightParameters.ColorTemp == null && toLightParameters.RgbColor == null)
            {
                toLightParameters = fromLightParameters with { Brightness = 0 };
            }
        }

        var result = fromLightParameters with
        {
            Brightness = InterpolateBrightness(fromLightParameters.Brightness, toLightParameters.Brightness, progress)
        };

        // If any of parameters has a rgb color, we use that for the result.
        if (fromLightParameters.RgbColor != null || toLightParameters.RgbColor != null)
        {
            var fromRgbColor = GetRgbColor(fromLightParameters);
            var toRgbColor = GetRgbColor(toLightParameters);
            var blendedColor = fromRgbColor.BlendWithGammaCorrection(toRgbColor, progress);

            return result with
            {
                RgbColor = blendedColor
            };
        }

        if (fromLightParameters.ColorTemp == null || toLightParameters.ColorTemp == null)
        {
            throw new InvalidOperationException("Interpolation requires either RGB or ColorTemp value");
        }

        var fromColorTemp = fromLightParameters.ColorTemp.Value;
        var toColorTemp = toLightParameters.ColorTemp.Value;
        // Note: This blending is done in Mired, which can be blended linearly. If we ever want to do this using Kelvin, this is no longer the case, and the blending should convert first.
        var blendedTemp = (int)Math.Round(fromColorTemp + (toColorTemp - fromColorTemp) * progress);

        return result with
        {
            ColorTemp = blendedTemp
        };
    }

    private static double? InterpolateBrightness(double? from, double? to, double progress)
    {
        if (from != null && to != null)
        {
            return Math.Round(from.Value + (to.Value - from.Value) * progress);
        }
        return from ?? to;
    }

    private static Color GetRgbColor(LightParameters lightParameters)
    {
        if (lightParameters.RgbColor != null)
        {
            return lightParameters.RgbColor.Value;
        }
        if (lightParameters.ColorTemp == null)
        {
            throw new ArgumentException("Rgb color conversion requires either RGB or ColorTemp value");
        }

        return ColorConverter.KelvinToRgb(1000000.0 / Convert.ToDouble(lightParameters.ColorTemp));
    }
}