using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Runtime;

namespace CodeCasa.NetDaemon.RuntimeState.Extensions;

/// <summary>
/// Extension methods for registering the <see cref="NetDaemonRuntimeStateService"/> in the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="NetDaemonRuntimeStateService"/> in the service collection.
    /// </summary>
    public static IServiceCollection AddNetDaemonRuntimeStateService(this IServiceCollection services)
    {
        services.VerifyNetDaemonDependencies();

        services.AddTransient<NetDaemonRuntimeStateService>();
        return services;
    }

    private static void VerifyNetDaemonDependencies(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();

        List<string> missing = [];
        if (serviceProvider.GetService<INetDaemonRuntime>() == null)
        {
            missing.Add(nameof(INetDaemonRuntime));
        }
        if (serviceProvider.GetService<IHomeAssistantRunner>() == null)
        {
            missing.Add(nameof(IHomeAssistantRunner));
        }

        if (missing.Any())
        {
            throw new InvalidOperationException(
                $"Cannot register {nameof(NetDaemonRuntimeStateService)}. Missing required services: {string.Join(", ", missing)}. " +
                $"Ensure these are registered by calling {nameof(HostBuilderExtensions.UseNetDaemonRuntime)} before calling {nameof(AddNetDaemonRuntimeStateService)}.");
        }
    }
}