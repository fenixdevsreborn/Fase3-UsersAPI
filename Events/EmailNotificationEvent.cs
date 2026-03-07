namespace ms_users.Events;

public class EmailNotificationEvent
{
  public required string Title { get; set; }

  public required string Subtitle { get; set; }

  public required string Body { get; set; }

  public required string Recipient { get; set; }

  public string? Sender { get; set; }
}