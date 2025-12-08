using System.Drawing;
using System.Text;

namespace NetDaemon.Lights;

/// <summary>
/// This record represents the state of a light entity.
/// Using a separate record rather than LightTurnOnParameters makes it easier to see what properties are being used. Especially as this record will also be used to store current state in later automations.
/// </summary>
public record LightParameters
{
    public double? Brightness { get; init; }
    public Color? RgbColor { get; init; }
    public int? ColorTemp { get; init; }

    public static LightParameters Off()
    {
        return new LightParameters { Brightness = 0 };
    }

    public static LightParameters On()
    {
        // Note: this is meant to be used by binary lights.
        return new LightParameters { Brightness = byte.MaxValue };
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        if (Brightness == 0)
        {
            sb.Append("off");
        }
        else
        {
            if (RgbColor != null)
            {
                sb.Append($"R,G,B: {RgbColor.Value.R},{RgbColor.Value.G},{RgbColor.Value.B}");
            }
            else if (ColorTemp != null)
            {
                sb.Append($"temperature {ColorTemp}");
            }

            if (sb.Length == 0)
            {
                sb.Append($"brightness {Brightness}");
            }
            else
            {
                sb.Append($" ({Brightness})");
            }
        }

        if (sb.Length == 0)
        {
            sb.Append("unknown light parameters");
        }
        return sb.ToString();
    }
}