using System.ComponentModel.DataAnnotations;

namespace Fcg.Users.Contracts.Users;

public class UpdateUserRequest
{
    [MinLength(1), MaxLength(200)]
    public string? Name { get; set; }

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [MinLength(6), MaxLength(500)]
    public string? Password { get; set; }

    [MaxLength(20)]
    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    [MaxLength(2000)]
    public string? AvatarUrl { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }
}
