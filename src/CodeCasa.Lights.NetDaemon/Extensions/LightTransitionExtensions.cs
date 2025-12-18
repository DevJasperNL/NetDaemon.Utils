using CodeCasa.Lights;
using CodeCasa.NetDaemon.Lights.Generated;

namespace CodeCasa.NetDaemon.Lights.Extensions;

internal static class LightTransitionExtensions
{
    public static LightTurnOnParameters ToLightTurnOnParameters(this LightTransition lightTransition)
    {
        return new LightTurnOnParameters
        {
            Transition = lightTransition.TransitionTime?.TotalSeconds,
            Brightness = lightTransition.LightParameters.Brightness,
            RgbColor = lightTransition.LightParameters.RgbColor?.ToRgbCollection(),
            ColorTemp = lightTransition.LightParameters.ColorTemp
        };
    }

    public static LightTurnOffParameters ToLightTurnOffParameters(this LightTransition lightTransition)
    {
        return new LightTurnOffParameters
        {
            Transition = lightTransition.TransitionTime?.TotalSeconds
        };
    }
}