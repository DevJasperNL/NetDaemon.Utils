using NetDaemon.Client;
using NetDaemon.Runtime;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace NetDaemon.RuntimeState;

/// <summary>
/// Service to monitor the runtime state of NetDaemon.
/// </summary>
public class NetDaemonRuntimeStateService : IDisposable
{
    private readonly BehaviorSubject<NetDaemonStates> _netDaemonConnected = new(NetDaemonStates.Initializing);

    private IDisposable? _connectionSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetDaemonRuntimeStateService"/> class.
    /// </summary>
    public NetDaemonRuntimeStateService(
        INetDaemonRuntime netDaemonRuntime,
        IHomeAssistantRunner homeAssistantRunner)
    {
        var connectionGate = Observable.FromAsync(async () =>
        {
            await netDaemonRuntime.WaitForInitializationAsync();
            return homeAssistantRunner.CurrentConnection != null
                ? NetDaemonStates.Connected
                : NetDaemonStates.Disconnected;
        });

        var postInitChanges =
            homeAssistantRunner.OnDisconnect.Select(_ => NetDaemonStates.Disconnected)
                .Merge(homeAssistantRunner.OnConnect.Select(_ => NetDaemonStates.Connected))
                .SkipUntil(connectionGate);

        var connectionChanges = connectionGate.Concat(postInitChanges);

        _connectionSubscription = connectionChanges.DistinctUntilChanged().Subscribe(_netDaemonConnected);
    }

    /// <summary>
    /// Gets the current state of NetDaemon.
    /// </summary>
    public NetDaemonStates NetDaemonState => _netDaemonConnected.Value;

    /// <summary>
    /// Returns an observable that emits the current state of NetDaemon and all subsequent changes.
    /// </summary>
    public IObservable<NetDaemonStates> ConnectedChangesWithCurrent() => _netDaemonConnected.AsObservable();

    /// <summary>
    /// Returns an observable that emits changes in the connection state of NetDaemon.
    /// </summary>
    public IObservable<NetDaemonStates> ConnectedChanges() => _netDaemonConnected.Skip(1).AsObservable();

    /// <summary>
    /// Asynchronously waits until NetDaemon has finished initializing.
    /// The task completes once NetDaemon transitions out of the <see cref="NetDaemonStates.Initializing"/> state.
    /// This is useful when performing entity operations outside NetDaemonApps, for example in a <c>BackgroundService</c>.
    /// </summary>
    public async Task WaitForInitializationAsync(CancellationToken cancellationToken)
    {
        await ConnectedChangesWithCurrent()
            .Where(connected => connected != NetDaemonStates.Initializing)
            .FirstAsync()
            .ToTask(cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_connectionSubscription == null)
        {
            return;
        }

        _connectionSubscription?.Dispose();
        _connectionSubscription = null;

        _netDaemonConnected.OnCompleted();
        _netDaemonConnected.Dispose();
    }
}