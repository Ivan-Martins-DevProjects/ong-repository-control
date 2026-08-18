namespace backend.Models;

public class NotificationEvent
{
    public int Id { get; set; }
    public string EventKey { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}