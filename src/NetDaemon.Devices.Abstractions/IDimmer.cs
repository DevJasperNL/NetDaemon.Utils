namespace NetDaemon.Devices.Abstractions;

public interface IDimmer
{
    public IObservable<bool> Dimming { get; }
    public IObservable<bool> Brightening { get; }
}