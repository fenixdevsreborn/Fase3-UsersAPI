using System.ComponentModel.DataAnnotations;

namespace Fcg.Users.Contracts.Users;

public class CreateUserRequest
{
    [Required, MinLength(1), MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(4), MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(500)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Role { get; set; } = "user";

    public bool IsActive { get; set; } = true;

    [MaxLength(2000)]
    public string? AvatarUrl { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }
}
