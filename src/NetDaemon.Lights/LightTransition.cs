
using NetDaemon.Lights.Extensions;

namespace NetDaemon.Lights;

public record LightTransition
{
    public LightParameters LightParameters { get; init; } = null!;
    public TimeSpan? TransitionTime { get; init; }

    public static LightTransition Off()
    {
        return LightParameters.Off().AsTransition();
    }

    public static LightTransition On()
    {
        return LightParameters.On().AsTransition();
    }

    public override string ToString()
    {
        return TransitionTime == null ? LightParameters.ToString() : $"{LightParameters} in {TransitionTime}";
    }
}