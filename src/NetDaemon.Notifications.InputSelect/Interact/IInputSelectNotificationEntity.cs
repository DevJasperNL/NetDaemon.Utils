﻿
using NetDaemon.Notifications.InputSelect.Config;

namespace NetDaemon.Notifications.InputSelect.Interact;

public interface IInputSelectNotificationEntity
{
    public string InputSelectEntityId { get; }
    public string? InputNumberEntityId { get; }
    InputSelectNotification Notify(IInputSelectNotificationConfig notification);
    InputSelectNotification Notify(IInputSelectNotificationConfig notification, InputSelectNotification notificationToReplace);
    InputSelectNotification Notify(IInputSelectNotificationConfig notification, string id);
    void RemoveNotification(InputSelectNotification notificationToRemove);
    void RemoveNotification(string id);
}