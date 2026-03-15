using System.ComponentModel.DataAnnotations;

namespace Fcg.Users.Contracts.Users;

public class UserListQuery
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(64)]
    public string? Username { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}
