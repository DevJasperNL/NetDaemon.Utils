namespace NetDaemon.Notifications.Phone.Config;

public record IosNotificationAction(Action Action, string Title) : NotificationAction(Action, Title)
{
    public string? Icon { get; set; }

    // Note: additional options available.

    public override object ToData(int index)
    {
        return new
        {
            action = $"{index}",
            title = Title,
            uri = Uri,
            icon = Icon
        };
    }
}