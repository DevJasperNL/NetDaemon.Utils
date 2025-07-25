﻿namespace NetDaemon.Notifications.InputSelect;

public class InputSelectNotification(string id, IDisposable disposable) : IDisposable
{
    public string Id { get; } = id;
    public void Dispose() => disposable.Dispose();
}