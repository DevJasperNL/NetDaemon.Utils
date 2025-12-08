using NetDaemon.HassModel.Entities;
using NetDaemon.Lights.Generated;
using NetDaemon.Lights.Scenes;
using NetDaemon.Lights.Utils;
using System.Drawing;

namespace NetDaemon.Lights.Extensions;

public static partial class LightEntityExtensions
{
    public static void TurnOn(this ILightEntityCore lightEntity, LightSceneTemplate lightSceneTemplate)
    {
        lightEntity.ExecuteLightTransition(lightSceneTemplate(lightEntity).AsTransition());
    }

    /// <summary>
    /// Determines whether a light entity is currently off.
    /// </summary>
    /// <param name="lightEntity">The light entity to check.</param>
    /// <returns>True if the light is off; otherwise, false.</returns>
    public static bool IsOff(this ILightEntityCore lightEntity) => EntityExtensions.IsOff(new LightEntity(lightEntity));

    /// <summary>
    /// Determines whether a light entity is currently on.
    /// </summary>
    /// <param name="lightEntity">The light entity to check.</param>
    /// <returns>True if the light is on; otherwise, false.</returns>
    public static bool IsOn(this ILightEntityCore lightEntity) => EntityExtensions.IsOn(new LightEntity(lightEntity));

    /// <summary>
    /// Turns a light entity on or off based on the provided boolean value.
    /// </summary>
    /// <param name="lightEntity">The light entity to control.</param>
    /// <param name="on">True to turn the light on; false to turn it off.</param>
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

    /// <summary>
    /// Executes a light transition on the specified light entity.
    /// If the transition results in brightness 0, the light is turned off; otherwise, it is turned on with the specified parameters.
    /// </summary>
    /// <param name="lightEntity">The light entity to apply the transition to.</param>
    /// <param name="lightTransition">The transition parameters to apply.</param>
    public static void ExecuteLightTransition(this ILightEntityCore lightEntity, LightTransition lightTransition)
    {
        if (lightTransition.LightParameters.Brightness == 0)
        {
            lightEntity.TurnOff(lightTransition.ToLightTurnOffParameters());
            return;
        }

        lightEntity.TurnOn(lightTransition.ToLightTurnOnParameters());
    }

    /// <summary>
    /// Gets the current brightness of a light entity.
    /// </summary>
    /// <param name="lightEntity">The light entity to get the brightness from.</param>
    /// <returns>The brightness value between 0 and 255, or 0 if the light is off.</returns>
    public static double GetBrightness(this ILightEntityCore lightEntity) => lightEntity.GetLightParameters().Brightness ?? 0;

    /// <summary>
    /// Gets the current light parameters (brightness, color, color temperature) of a light entity.
    /// </summary>
    /// <param name="lightEntity">The light entity to get parameters from.</param>
    /// <returns>A <see cref="LightParameters"/> object representing the current state of the light.</returns>
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
    /// Flattens a light entity by recursively resolving all its child lights.
    /// If the light is a group or composite light with children, returns all leaf light entities.
    /// If the light has no children, returns the light entity itself.
    /// </summary>
    /// <param name="lightEntity">The light entity to flatten.</param>
    /// <returns>An array of light entities representing all leaf light entities.</returns>
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

    /// <summary>
    /// Determines whether a light entity currently matches the specified light scene parameters.
    /// Brightness values are allowed to deviate by 1 from the set value.
    /// </summary>
    /// <param name="lightEntity">The light entity to check.</param>
    /// <param name="lightParameters">The expected light parameters.</param>
    /// <returns>True if the light's current parameters match the specified parameters (within tolerance); otherwise, false.</returns>
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