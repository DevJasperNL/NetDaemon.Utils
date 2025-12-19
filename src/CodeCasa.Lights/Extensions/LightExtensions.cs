namespace CodeCasa.Lights.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ILight"/> instances.
/// </summary>
/// <remarks>
/// This class contains a collection of utility methods to simplify light operations,
/// including turning lights on/off, applying transitions, retrieving parameters, checking state, and flattening light hierarchies.
/// All methods are extension methods on <see cref="ILight"/>, allowing for a fluent API.
/// </remarks>
public static class LightExtensions
{
    /// <summary>
    /// Turns on a light with the specified light parameters.
    /// </summary>
    /// <param name="light">The light to turn on.</param>
    /// <param name="lightParameters">The light parameters (brightness, color, etc.) to apply.</param>
    public static void TurnOn(this ILight light, LightParameters lightParameters)
    {
        light.ApplyTransition(lightParameters.AsTransition());
    }

    /// <summary>
    /// Turns on a light with the specified light parameters and a custom transition time.
    /// </summary>
    /// <param name="light">The light to turn on.</param>
    /// <param name="lightParameters">The light parameters (brightness, color, etc.) to apply.</param>
    /// <param name="transitionTime">The duration of the transition.</param>
    public static void TurnOn(this ILight light, LightParameters lightParameters, TimeSpan transitionTime)
    {
        light.ApplyTransition(lightParameters.AsTransition(transitionTime));
    }

    /// <summary>
    /// Turns on a light with the specified light transition.
    /// </summary>
    /// <param name="light">The light to turn on.</param>
    /// <param name="lightTransition">The transition parameters to apply.</param>
    public static void TurnOn(this ILight light, LightTransition lightTransition)
    {
        light.ApplyTransition(lightTransition);
    }

    /// <summary>
    /// Turns on a light with default parameters.
    /// </summary>
    /// <param name="light">The light to turn on.</param>
    public static void TurnOn(this ILight light)
    {
        light.ApplyTransition(LightTransition.On());
    }

    /// <summary>
    /// Turns on a light with a custom transition time.
    /// </summary>
    /// <param name="light">The light to turn on.</param>
    /// <param name="transitionTime">The duration of the transition.</param>
    public static void TurnOn(this ILight light, TimeSpan transitionTime)
    {
        light.ApplyTransition(LightTransition.On(transitionTime));
    }

    /// <summary>
    /// Turns off a light with a custom transition time.
    /// </summary>
    /// <param name="light">The light to turn off.</param>
    /// <param name="transitionTime">The duration of the transition.</param>
    public static void TurnOff(this ILight light, TimeSpan transitionTime)
    {
        light.ApplyTransition(LightTransition.Off(transitionTime));
    }

    /// <summary>
    /// Turns off a light with default parameters.
    /// </summary>
    /// <param name="light">The light to turn off.</param>
    public static void TurnOff(this ILight light)
    {
        light.ApplyTransition(LightTransition.Off());
    }

    /// <summary>
    /// Determines whether the light is on by checking if brightness is greater than zero.
    /// </summary>
    /// <param name="light">The light to check.</param>
    /// <returns><c>true</c> if the light's brightness is greater than zero; otherwise, <c>false</c>.</returns>
    public static bool IsOn(this ILight light)
    {
        return (light.GetParameters().Brightness ?? 0) != 0;
    }

    /// <summary>
    /// Determines whether the light is off by checking if brightness is zero.
    /// </summary>
    /// <param name="light">The light to check.</param>
    /// <returns><c>true</c> if the light's brightness is zero; otherwise, <c>false</c>.</returns>
    public static bool IsOff(this ILight light)
    {
        return (light.GetParameters().Brightness ?? 0) == 0;
    }

    /// <summary>
    /// Gets the current brightness of a light.
    /// </summary>
    /// <param name="light">The light to get the brightness from.</param>
    /// <returns>The brightness value, or 0 if the light is off.</returns>
    public static double GetBrightness(this ILight light) => light.GetParameters().Brightness ?? 0;

    /// <summary>
    /// Flattens a light hierarchy into a single-dimensional array of leaf lights.
    /// </summary>
    /// <remarks>
    /// This method recursively traverses the light hierarchy and collects all leaf lights
    /// (lights without children). Circular references are handled by tracking visited lights.
    /// </remarks>
    /// <param name="light">The light to flatten.</param>
    /// <returns>An array of leaf lights from the light hierarchy.</returns>
    public static ILight[] Flatten(this ILight light)
    {
        var visitedLights = new HashSet<ILight>();
        var result = new List<ILight>();
        FlattenRecursive(light, visitedLights, result);
        return result.ToArray();
    }

    /// <summary>
    /// Recursively flattens a light hierarchy and its children, collecting all leaf lights.
    /// This method prevents infinite loops by tracking visited lights.
    /// </summary>
    /// <param name="light">The light to process.</param>
    /// <param name="visitedLights">A set of lights that have already been visited to prevent circular references.</param>
    /// <param name="result">The collection to accumulate the leaf lights.</param>
    private static void FlattenRecursive(ILight light, HashSet<ILight> visitedLights, List<ILight> result)
    {
        if (!visitedLights.Add(light))
        {
            return;
        }

        var children = light.GetChildren();
        if (!children.Any())
        {
            result.Add(light);
            return;
        }
        foreach (var child in children)
        {
            FlattenRecursive(child, visitedLights, result);
        }
    }
}