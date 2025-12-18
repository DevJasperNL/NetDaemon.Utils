
namespace CodeCasa.NetDaemon.Extensions.Observables;

internal static class Constants
{
    // Note: For simplicity these values are now hardcoded while the actual values are not documented for Home Assistant and might depend on individual plugins. These might become configurable in the future, but for now I didn't want to overengineer.
    public static string[] EntityUnavailableStates = ["unknown", "unavailable"];
}