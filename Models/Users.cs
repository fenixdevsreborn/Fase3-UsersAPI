namespace ms_users.Models;

public class Users
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public string Email { get; set; }
  public string Password { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}