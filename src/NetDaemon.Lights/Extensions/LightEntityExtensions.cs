using System.Drawing;
using NetDaemon.HassModel.Entities;
using NetDaemon.Lights.Generated;
using NetDaemon.Lights.Utils;

namespace NetDaemon.Lights.Extensions;

public static class LightEntityExtensions
{
    private const int RelaxTemp = 500;
    private const int WarmTemp = 366;
    private const int ConcentrateTemp = 233;
    private const int WhiteTemp = 153;

    public static bool IsOff(this ILightEntityCore lightEntity) => EntityExtensions.IsOff(new LightEntity(lightEntity));
    public static bool IsOn(this ILightEntityCore lightEntity) => EntityExtensions.IsOn(new LightEntity(lightEntity));

    public static void TurnOnOff(this ILightEntityCore lightEntity, bool on)
    {
        if (on)
        {
            lightEntity.TurnOn();
        }
        else
        {
            lightEntity.TurnOff();
        }
    }

    public static void ExecuteLightTransition(this ILightEntityCore lightEntity, LightTransition lightTransition)
    {
        if (lightTransition.LightParameters.Brightness == 0)
        {
            lightEntity.TurnOff(lightTransition.ToLightTurnOffParameters());
            return;
        }

        lightEntity.TurnOn(lightTransition.ToLightTurnOnParameters());
    }

    public static double GetBrightness(this ILightEntityCore lightEntity) => lightEntity.GetLightParameters().Brightness ?? 0;

    public static LightParameters GetLightParameters(this ILightEntityCore lightEntity)
    {
        var entity = new LightEntity(lightEntity);
        if (entity.IsOff())
        {
            return LightParameters.Off();
        }
        var attributes = entity.Attributes;
        if (attributes == null)
        {
            return LightParameters.Off(); // This should not occur.
        }

        return attributes.ToLightParameters();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the relax scene of any given light entity.
    /// </summary>
    public static LightParameters GetRelaxSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(RelaxTemp, 142) ??
               lightEntity.TryGetBrightnessParameters(142) ?? // Note: This will also turn on white lights that only support brightness, but we don't have those.
               LightParameters.Off();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the nightlight scene of any given light entity.
    /// </summary>
    public static LightParameters GetNightLightSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(RelaxTemp, 2) ??
               lightEntity.TryGetBrightnessParameters(142) ?? // Note: This will also turn on white lights that only support brightness, but we don't have those.
               LightParameters.Off();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the concentrate scene of any given light entity.
    /// </summary>
    public static LightParameters GetConcentrateSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(ConcentrateTemp, byte.MaxValue) ??
               lightEntity.TryGetOnOffOnParameters() ??
               LightParameters.Off();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the bright scene of any given light entity.
    /// </summary>
    public static LightParameters GetBrightSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(WarmTemp, byte.MaxValue) ??
               lightEntity.TryGetOnOffOnParameters() ??
               LightParameters.Off();
    }

    /// <summary>
    /// Will attempt to autogenerate the parameters for the dimmed scene of any given light entity.
    /// </summary>
    public static LightParameters GetDimmedSceneParameters(this ILightEntityCore lightEntity)
    {
        return lightEntity.TryGetColorTempParameters(WarmTemp, 76) ?? 
               LightParameters.Off();
    }

    public static LightParameters GetWarningSceneParameters(this ILightEntityCore lightEntity, bool high)
    {
        return GetWarningOrAlarmSceneParameters(lightEntity, true, high);
    }

    public static LightParameters GetAlarmSceneParameters(this ILightEntityCore lightEntity, bool high)
    {
        return GetWarningOrAlarmSceneParameters(lightEntity, false, high);
    }

    private static LightParameters GetWarningOrAlarmSceneParameters(ILightEntityCore lightEntity, bool warning, bool high)
    {
        int brightness = high ? byte.MaxValue : byte.MaxValue / 2;
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

    private static LightParameters? TryGetColorParameters(this ILightEntityCore lightEntity, Color color, int brightness)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.Xy))
        {
            return null;
        }

        return new() { RgbColor = color, Brightness = brightness };
    }

    private static LightParameters? TryGetColorTempParameters(this ILightEntityCore lightEntity, int desiredTemp, int brightness)
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

    public static bool SupportsColorMode(this ILightEntityCore lightEntity, string colorMode)
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

    private static LightParameters? TryGetBrightnessParameters(this ILightEntityCore lightEntity, int brightness)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.Brightness))
        {
            return null;
        }
        return new() { Brightness = brightness };
    }

    private static LightParameters? TryGetOnOffOnParameters(this ILightEntityCore lightEntity)
    {
        if (!lightEntity.SupportsColorMode(ColorModes.OnOff))
        {
            return null;
        }
        return new() { Brightness = byte.MaxValue };
    }

    public static int? GetColorTempWithinRange(this ILightEntityCore lightEntity, int desiredMireds)
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

    public static ILightEntityCore[] Flatten(this ILightEntityCore lightEntity)
    {
        var entity = new LightEntity(lightEntity);
        var visitedEntities = new HashSet<string>();
        var result = new List<ILightEntityCore>();
        FlattenRecursive(entity, visitedEntities, result);
        return result.ToArray();
    }

    private static void FlattenRecursive(LightEntity lightEntity, HashSet<string> visitedEntities, List<ILightEntityCore> result)
    {
        if (!visitedEntities.Add(lightEntity.EntityId))
        {
            return;
        }

        var children = lightEntity.Attributes?.EntityId;
        if (children == null || !children.Any())
        {
            result.Add(lightEntity);
            return;
        }
        foreach (var entityId in children)
        {
            var nestedLightEntity = new LightEntity(lightEntity.HaContext, entityId);
            FlattenRecursive(nestedLightEntity, visitedEntities, result);
        }
    }

    public static bool SceneEquals(this ILightEntityCore lightEntity, LightParameters lightParameters)
    {
        var actualParameters = lightEntity.GetLightParameters();
        return
            BrightnessEquals(actualParameters.Brightness, lightParameters.Brightness) &&
            actualParameters.ColorTemp == lightParameters.ColorTemp;
    }

    /// <summary>
    /// Actual brightness can deviate 1 from the set brightness. This method allows for that deviation.
    /// </summary>
    /// <param name="brightness1"></param>
    /// <param name="brightness2"></param>
    /// <returns></returns>
    private static bool BrightnessEquals(double? brightness1, double? brightness2)
    {
        if (brightness1 == null && brightness2 == null)
        {
            return true;
        }
        if (brightness1 != null && brightness2 == null || brightness1 == null && brightness2 != null)
        {
            return false;
        }

        const long maxDeviation = 1;
        var b1 = brightness1!.Value;
        var b2 = brightness2!.Value;
        return b1 - maxDeviation <= b2 && b1 + maxDeviation >= b2;
    }
}