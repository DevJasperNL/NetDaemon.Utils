﻿using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using NetDaemon.HassModel;
using NetDaemon.Notifications.Phone.Config;

namespace NetDaemon.Notifications.Phone;

public class PhoneNotificationEntity : IDisposable
{
    public delegate void MobileAppNotificationDelegate(string message, string? title = null, object? target = null, object? data = null);

    private readonly MobileAppNotificationDelegate _notify;

    private const string MobileAppNotificationAction = "mobile_app_notification_action";
    private const string ClearNotification = "clear_notification";

    private readonly Lock _lock = new();
    private readonly Dictionary<string, Action[]> _actions = new();
    private readonly IDisposable _eventDisposable;

    protected PhoneNotificationEntity(IHaContext haContext, MobileAppNotificationDelegate notifyMethod)
    {
        _notify = notifyMethod;
        _eventDisposable = haContext.Events.Filter<PhoneActionEventData>(MobileAppNotificationAction)
            .Where(e => e.Data != null)
            .Subscribe(e =>
            {
                lock (_lock)
                {
                    if (e.Data?.Tag == null || !_actions.TryGetValue(e.Data.Tag, out var actions))
                    {
                        return;
                    }

                    if (e.Data?.Action == null || !int.TryParse(e.Data?.Action, out int actionId) || actionId < 0 ||
                        actionId >= actions.Length)
                    {
                        return;
                    }

                    actions[actionId].Invoke();
                }
            });
    }

    public PhoneNotification Notify(PhoneNotificationConfig config)
    {
        return Notify(config, Guid.NewGuid().ToString());
    }

    public PhoneNotification Notify(PhoneNotificationConfig config, PhoneNotification notificationToReplace)
    {
        return Notify(config, notificationToReplace.Id);
    }

    public PhoneNotification Notify(PhoneNotificationConfig config, string id)
    {
        lock (_lock)
        {
            if (config.Actions != null)
            {
                _actions[id] = config.Actions.Select(a => a.Action).ToArray();
            }
            else
            {
                _actions.Remove(id);
            }
        }

        var data = config.ToData(id);
        _notify(config.Message, config.Title, data: data);

        return new PhoneNotification(id, Disposable.Create(() => RemoveNotification(id)));
    }

    public void RemoveNotification(PhoneNotification notificationToRemove)
    {
        RemoveNotification(notificationToRemove.Id);
    }

    public void RemoveNotification(string id)
    {
        _notify(ClearNotification, data: new { tag = id });
        lock (_lock)
        {
            _actions.Remove(id);
        }
    }

    private record PhoneActionEventData
    {
        [JsonPropertyName("tag")] public string? Tag { get; init; }
        [JsonPropertyName("action")] public string? Action { get; init; }
    }

    public void Dispose()
    {
        _eventDisposable.Dispose();
    }
}