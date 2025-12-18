namespace CodeCasa.NetDaemon.RuntimeState;

/// <summary>
/// Represents the possible states of NetDaemon.
/// </summary>
public enum NetDaemonStates
{
    /// <summary>
    /// Indicates that NetDaemon is initializing and yet to build its state cache. While in this state, entities cannot be used.
    /// </summary>
    Initializing,

    /// <summary>
    /// Indicates that NetDaemon is connected to Home Assistant.
    /// </summary>
    Connected,

    /// <summary>
    /// Indicates that NetDaemon is disconnected from Home Assistant. Cached entity states are still available, but no updates will be received until reconnected.
    /// </summary>
    Disconnected
}