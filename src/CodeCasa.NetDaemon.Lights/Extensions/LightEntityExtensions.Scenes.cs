using System.Drawing;
using CodeCasa.NetDaemon.Lights.Generated;
using CodeCasa.NetDaemon.Lights.Utils;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Lights.Extensions;

/// <summary>
/// Provides extension methods for light entities to support scene generation and light parameter operations.
/// </summary>
public static partial class LightEntityExtensions
{
    private const int RelaxTemp = 500;
    private const int WarmTemp = 366;
    private const int ConcentrateTemp = 233;
    private const int WhiteTemp = 153;

    /// <summary>
    /// Will attempt to autogenerate the parameters for the relax scene of any given light entity.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <returns>Light parameters suitable for a relax scene, or <see cref="LightParameters.Off()"/> if the light does not support any appropriate color mode.</returns>
    public static LightParameters GetRelaxSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(RelaxTemp, 142) ??
               lightEntity.TryGetBrightnessParameters(
                   142) ?? // Note: This will also turn on white lights that only support brightness, but we don't have those.
               LightParameters.Off();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the nightlight scene of any given light entity.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <returns>Light parameters suitable for a nightlight scene, or <see cref="LightParameters.Off()"/> if the light does not support any appropriate color mode.</returns>
    public static LightParameters GetNightLightSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(RelaxTemp, 2) ??
               lightEntity.TryGetBrightnessParameters(
                   142) ?? // Note: This will also turn on white lights that only support brightness, but we don't have those.
               LightParameters.Off();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the concentrate scene of any given light entity.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <returns>Light parameters suitable for a concentrate scene, or <see cref="LightParameters.Off()"/> if the light does not support any appropriate color mode.</returns>
    public static LightParameters GetConcentrateSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(ConcentrateTemp, byte.MaxValue) ??
               lightEntity.TryGetOnOffOnParameters() ??
               LightParameters.Off();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the bright scene of any given light entity.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <returns>Light parameters suitable for a bright scene, or <see cref="LightParameters.Off()"/> if the light does not support any appropriate color mode.</returns>
    public static LightParameters GetBrightSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(WarmTemp, byte.MaxValue) ??
               lightEntity.TryGetOnOffOnParameters() ??
               LightParameters.Off();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the dimmed scene of any given light entity.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <returns>Light parameters suitable for a dimmed scene, or <see cref="LightParameters.Off()"/> if the light does not support any appropriate color mode.</returns>
    public static LightParameters GetDimmedSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(WarmTemp, 76) ??
               LightParameters.Off();
    }

    /// <summary>
    /// Generates light parameters for a warning scene with the specified intensity level.
    /// Attempts to use color (red), color temperature, or on/off modes based on light capabilities.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <param name="high">If true, uses maximum intensity; if false, uses half intensity.</param>
    /// <returns>Light parameters suitable for a warning scene, or <see cref="LightParameters.Off()"/> if the light does not support any appropriate color mode.</returns>
    public static LightParameters GetWarningSceneParameters(this ILightEntityCore lightEntity, bool high)
    {
        var brightness = high ? byte.MaxValue : byte.MaxValue / 2;
        var colorParams = lightEntity.TryGetColorParameters(Color.Red, brightness);
        if (colorParams != null)
        {
            return colorParams;
        }

        var colorTempParams = lightEntity.TryGetColorTempParameters(WhiteTemp, brightness);
        if (colorTempParams != null)
        {
            return colorTempParams;
        }

        var onOffParams = lightEntity.TryGetOnOffOnParameters();
        if (high && onOffParams != null)
        {
            return onOffParams;
        }

        return LightParameters.Off();
    }

    /// <summary>
    /// Attempts to create light parameters using an RGB color and brightness level, if the light supports color mode.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <param name="color">The RGB color to use.</param>
    /// <param name="brightness">The brightness level to set.</param>
    /// <returns>Light parameters with the specified color and brightness, or <c>null</c> if the light does not support the color mode.</returns>
    private static LightParameters? TryGetColorParameters(this ILightEntityCore lightEntity, Color color,
        int brightness)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.Xy))
        {
            return null;
        }

        return new() { RgbColor = color, Brightness = brightness };
    }

    /// <summary>
    /// Attempts to create light parameters using a color temperature and brightness level, if the light supports color temperature mode.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <param name="desiredTemp">The desired color temperature in Mireds.</param>
    /// <param name="brightness">The brightness level to set.</param>
    /// <returns>Light parameters with the color temperature clamped to the light's supported range and the specified brightness, or <c>null</c> if the light does not support color temperature mode.</returns>
    private static LightParameters? TryGetColorTempParameters(this ILightEntityCore lightEntity, int desiredTemp,
        int brightness)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.ColorTemp))
        {
            return null;
        }

        var colorTemp = lightEntity.GetColorTempWithinRange(desiredTemp);
        if (colorTemp == null)
        {
            return null;
        }

        return new() { ColorTemp = colorTemp, Brightness = brightness };
    }

    /// <summary>
    /// Determines whether the light entity supports a specific color mode.
    /// </summary>
    /// <param name="lightEntity">The light entity to check.</param>
    /// <param name="colorMode">The color mode to check for.</param>
    /// <returns><c>true</c> if the light supports the specified color mode; otherwise, <c>false</c>.</returns>
    private static bool SupportsColorMode(this ILightEntityCore lightEntity, string colorMode)
    {
        var entity = new LightEntity(lightEntity);
        var attributes = entity.Attributes;
        if (attributes?.SupportedColorModes == null)
        {
            return false;
        }

        return attributes.SupportedColorModes.Any(mode =>
            string.Equals(mode, colorMode, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Attempts to create light parameters using only brightness level, if the light supports brightness mode.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <param name="brightness">The brightness level to set.</param>
    /// <returns>Light parameters with the specified brightness, or <c>null</c> if the light does not support brightness mode.</returns>
    private static LightParameters? TryGetBrightnessParameters(this ILightEntityCore lightEntity, int brightness)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.Brightness))
        {
            return null;
        }

        return new() { Brightness = brightness };
    }

    /// <summary>
    /// Attempts to create light parameters for on/off mode with maximum brightness, if the light supports on/off mode.
    /// </summary>
    /// <param name="lightEntity">The light entity to generate parameters for.</param>
    /// <returns>Light parameters with maximum brightness set for on/off mode, or <c>null</c> if the light does not support on/off mode.</returns>
    private static LightParameters? TryGetOnOffOnParameters(this ILightEntityCore lightEntity)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.OnOff))
        {
            return null;
        }

        return new() { Brightness = byte.MaxValue };
    }

    /// <summary>
    /// Gets a color temperature value clamped to the light's minimum and maximum supported color temperature range.
    /// </summary>
    /// <param name="lightEntity">The light entity to check color temperature range for.</param>
    /// <param name="desiredMireds">The desired color temperature in Mireds.</param>
    /// <returns>The color temperature clamped to the light's supported range, or <c>null</c> if the light does not have a color temperature range defined.</returns>
    private static int? GetColorTempWithinRange(this ILightEntityCore lightEntity, int desiredMireds)
    {
        var entity = new LightEntity(lightEntity);
        var attributes = entity.Attributes;
        if (attributes == null)
        {
            return null;
        }

        if (attributes.MinMireds == null || attributes.MaxMireds == null)
        {
            return null;
        }

        return Math.Min((int)attributes.MaxMireds, Math.Max((int)attributes.MinMireds, desiredMireds));
    }
}