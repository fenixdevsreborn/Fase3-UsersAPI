namespace ms_users.Models
{
    public class RegisterRequestUser
    {
      public required string Email { get; init; }
      public required string Password { get; init; }
      public required string Nickname { get; init; }
      public required string Name { get; init; }
    }
}
