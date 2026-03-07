namespace ms_users.Events;

public class UserRegisteredEvent
{
  public string EventType => "USER_REGISTERED";

  public string UserId { get; set; }

  public string Email { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
